using BloogBot.AI;
using BloogBot.Game;
using Bootstrapper;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Input;
using static BloogBot.UI.WinImports;

namespace BloogBot.UI
{
    public class MainViewModel : INotifyPropertyChanged
    {
        const string COMMAND_ERROR = "An error occured. See Console for details.";

        static readonly string[] CityNames = { "Orgrimmar", "Thunder Bluff", "Undercity", "Stormwind", "Darnassus", "Ironforge" };

        readonly BotLoader botLoader = new BotLoader();
        readonly Probe probe;
        readonly BotSettings botSettings;
        bool readyForCommands;

        public IntPtr processPointer;

        public MainViewModel()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var botSettingsFilePath = Path.Combine(currentFolder, "botSettings.json");
            botSettings = JsonConvert.DeserializeObject<BotSettings>(File.ReadAllText(botSettingsFilePath));
            UpdatePropertiesWithAttribute(typeof(BotSettingAttribute));

            Logger.Initialize(botSettings);
            SqliteRepository.Initialize();
            DiscordClientWrapper.Initialize(botSettings);

            void callback()
            {
                UpdatePropertiesWithAttribute(typeof(ProbeFieldAttribute));
            }
            void killswitch()
            {
                Stop();
            }
            probe = new Probe(callback, killswitch)
            {

            };

            InitializeObjectManager();

            ReloadBots();
        }

        public ObservableCollection<string> WoWProcessList { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ConsoleOutput { get; } = new ObservableCollection<string>();
        public ObservableCollection<IBot> Bots { get; private set; }

        #region Commands

        // Start command
        ICommand startCommand;

        void UiStart()
        {
            Start();
            Log("Bot started!");
        }

        void Start()
        {
            try
            {
                ReloadBots();

                var container = CurrentBot.GetDependencyContainer(botSettings, probe);

                void stopCallback()
                {
                    OnPropertyChanged(nameof(StartAllCommandEnabled));
                    OnPropertyChanged(nameof(StopAllCommandEnabled));
                    OnPropertyChanged(nameof(ReloadBotsCommandEnabled));
                }

                currentBot.Start(container, stopCallback);

                OnPropertyChanged(nameof(StartAllCommandEnabled));
                OnPropertyChanged(nameof(StopAllCommandEnabled));
                OnPropertyChanged(nameof(ReloadBotsCommandEnabled));
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand StartCommand =>
            startCommand ?? (startCommand = new CommandHandler(UiStart, true));

        // Stop command
        ICommand stopCommand;

        void UiStop()
        {
            Stop();
            Log("Bot stopped!");
        }

        void Stop()
        {
            try
            {
                var container = CurrentBot.GetDependencyContainer(botSettings, probe);

                currentBot.Stop();

                OnPropertyChanged(nameof(StartAllCommandEnabled));
                OnPropertyChanged(nameof(StopAllCommandEnabled));
                OnPropertyChanged(nameof(ReloadBotsCommandEnabled));
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        public ICommand StopCommand =>
            stopCommand ?? (stopCommand = new CommandHandler(UiStop, true));

        // ReloadBot command
        ICommand reloadBotsCommand;

        void ReloadBots()
        {
            try
            {
                Bots = new ObservableCollection<IBot>(botLoader.ReloadBots());

                CurrentBot = Bots.FirstOrDefault(b => b.Name == botSettings.CurrentBotName) ?? Bots.First();

                OnPropertyChanged(nameof(Bots));
                OnPropertyChanged(nameof(WoWProcessList));
                OnPropertyChanged(nameof(StartAllCommandEnabled));
                OnPropertyChanged(nameof(StopAllCommandEnabled));
                OnPropertyChanged(nameof(ReloadBotsCommandEnabled));

                Log("Bot successfully loaded!");
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand ReloadBotsCommand =>
            reloadBotsCommand ?? (reloadBotsCommand = new CommandHandler(ReloadBots, true));

        // ClearLog
        ICommand clearLogCommand;

        void ClearLog()
        {
            try
            {
                ConsoleOutput.Clear();
                OnPropertyChanged(nameof(ConsoleOutput));
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand ClearLogCommand =>
            clearLogCommand ?? (clearLogCommand = new CommandHandler(ClearLog, true));

        // NewWoWInstance command
        ICommand newWoWInstanceCommand;

        void SpawnNewWowInstance()
        {
            try
            {
                var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var bootstrapperSettingsFilePath = Path.Combine(currentFolder, "bootstrapperSettings.json");
                var bootstrapperSettings = JsonConvert.DeserializeObject<BootstrapperSettings>(File.ReadAllText(bootstrapperSettingsFilePath));

                var startupInfo = new STARTUPINFO();

                // run BloogBot.exe in a new process
                CreateProcess(
                    bootstrapperSettings.PathToWoW,
                    null,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    ProcessCreationFlag.CREATE_DEFAULT_ERROR_MODE,
                    IntPtr.Zero,
                    null,
                    ref startupInfo,
                    out PROCESS_INFORMATION processInfo);

                // this seems to help prevent timing issues
                Thread.Sleep(1000);

                // get a handle to the BloogBot process
                var processHandle = Process.GetProcessById((int)processInfo.dwProcessId).Handle;

                // resolve the file path to Loader.dll relative to our current working directory
                var loaderPath = Path.Combine(currentFolder, "Loader.dll");

                // allocate enough memory to hold the full file path to Loader.dll within the BloogBot process
                var loaderPathPtr = VirtualAllocEx(
                    processHandle,
                    (IntPtr)0,
                    loaderPath.Length,
                    MemoryAllocationType.MEM_COMMIT,
                    MemoryProtectionType.PAGE_EXECUTE_READWRITE);

                // this seems to help prevent timing issues
                Thread.Sleep(500);

                int error = Marshal.GetLastWin32Error();
                if (error > 0)
                    throw new InvalidOperationException($"Failed to allocate memory for Loader.dll, error code: {error}");

                // write the file path to Loader.dll to the EoE process's memory
                var bytes = Encoding.Unicode.GetBytes(loaderPath);
                var bytesWritten = 0; // throw away
                WriteProcessMemory(processHandle, loaderPathPtr, bytes, bytes.Length, ref bytesWritten);

                // this seems to help prevent timing issues
                Thread.Sleep(1000);

                error = Marshal.GetLastWin32Error();
                if (error > 0 || bytesWritten == 0)
                    throw new InvalidOperationException($"Failed to write Loader.dll into the WoW.exe process, error code: {error}");

                // search current process's for the memory address of the LoadLibraryW function within the kernel32.dll module
                var loaderDllPointer = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");

                // this seems to help prevent timing issues
                Thread.Sleep(1000);

                error = Marshal.GetLastWin32Error();
                if (error > 0)
                    throw new InvalidOperationException($"Failed to get memory address to Loader.dll in the WoW.exe process, error code: {error}");

                // create a new thread with the execution starting at the LoadLibraryW function, 
                // with the path to our Loader.dll passed as a parameter
                processPointer = CreateRemoteThread(processHandle, (IntPtr)null, (IntPtr)0, loaderDllPointer, loaderPathPtr, 0, (IntPtr)null);

                // this seems to help prevent timing issues
                Thread.Sleep(1000);

                error = Marshal.GetLastWin32Error();
                if (error > 0)
                    throw new InvalidOperationException($"Failed to create remote thread to start execution of Loader.dll in the WoW.exe process, error code: {error}");

                // free the memory that was allocated by VirtualAllocEx
                VirtualFreeEx(processHandle, loaderPathPtr, 0, MemoryFreeType.MEM_RELEASE);
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand NewWoWInstanceCommand =>
            newWoWInstanceCommand ?? (newWoWInstanceCommand = new CommandHandler(SpawnNewWowInstance, true));
        #endregion

        #region Observables
        public bool StartAllCommandEnabled => !currentBot.Running();

        public bool StopAllCommandEnabled => currentBot.Running();

        public bool ReloadBotsCommandEnabled => !currentBot.Running();

        // General
        IBot currentBot;
        public IBot CurrentBot
        {
            get => currentBot;
            set
            {
                currentBot = value;
                OnPropertyChanged(nameof(CurrentBot));
            }
        }

        // ProbeFields
        [ProbeField]
        public string CurrentState
        {
            get => probe.CurrentState;
        }

        [ProbeField]
        public string CurrentPosition
        {
            get => probe.CurrentPosition;
        }

        [ProbeField]
        public string CurrentZone
        {
            get => probe.CurrentZone;
        }

        [ProbeField]
        public string TargetName
        {
            get => probe.TargetName;
        }

        [ProbeField]
        public string TargetClass
        {
            get => probe.TargetClass;
        }

        [ProbeField]
        public string TargetCreatureType
        {
            get => probe.TargetCreatureType;
        }

        [ProbeField]
        public string TargetPosition
        {
            get => probe.TargetPosition;
        }

        [ProbeField]
        public string TargetRange
        {
            get => probe.TargetRange;
        }

        [ProbeField]
        public string TargetFactionId
        {
            get => probe.TargetFactionId;
        }

        [ProbeField]
        public string TargetIsCasting
        {
            get => probe.TargetIsCasting;
        }

        [ProbeField]
        public string TargetIsChanneling
        {
            get => probe.TargetIsChanneling;
        }

        [ProbeField]
        public string UpdateLatency
        {
            get => probe.UpdateLatency;
        }

        [ProbeField]
        public string CurrentQuestName
        {
            get => probe.CurrentQuestName;
        }

        [ProbeField]
        public string CurrentTask
        {
            get => probe.CurrentTask;
        }

        [ProbeField]
        public string TargetGuid
        {
            get => probe.TargetGuid;
        }

        [ProbeField]
        public string TargetID
        {
            get => probe.TargetID;
        }

        // BotSettings
        [BotSetting]
        public bool UseVerboseLogging
        {
            get => botSettings.UseVerboseLogging;
            set
            {
                botSettings.UseVerboseLogging = value;
                OnPropertyChanged(nameof(UseVerboseLogging));
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void Log(string message) =>
            ConsoleOutput.Add($"({DateTime.Now.ToShortTimeString()}) {message}");

        public void InitializeObjectManager()
        {
            ObjectManager.Initialize(probe);
            ObjectManager.StartEnumeration();
        }

        void UpdatePropertiesWithAttribute(Type type)
        {
            foreach (var propertyInfo in GetType().GetProperties())
            {
                if (Attribute.IsDefined(propertyInfo, type))
                    OnPropertyChanged(propertyInfo.Name);
            }
        }

        void OnChatMessageCallback(object sender, OnChatMessageArgs e)
        {
            var player = ObjectManager.Player;
            if (player != null && !CityNames.Contains(ObjectManager.ZoneText))
            {
                if (e.ChatChannel == "Say")
                    DiscordClientWrapper.SendMessage($"{player.Name} saw a chat message from {e.UnitName}: {e.Message}");
                else if (e.ChatChannel == "Whisper")
                    DiscordClientWrapper.SendMessage($"{player.Name} received a whisper from {e.UnitName}: {e.Message}");
            }
        }
    }

    public class BotSettingAttribute : Attribute
    {
    }

    public class ProbeFieldAttribute : Attribute
    {
    }
}