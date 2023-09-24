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
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Input;
using BloogBot.Models.Dto;
using static BloogBot.UI.WinImports;
using System.Windows.Threading;

namespace BloogBot.UI
{
    public class MainViewModel : INotifyPropertyChanged
    {
        const string COMMAND_ERROR = "An error occured. See Console for details.";

        static readonly string[] CityNames = { "Orgrimmar", "Thunder Bluff", "Undercity", "Stormwind", "Darnassus", "Ironforge" };

        readonly BotLoader botLoader = new BotLoader();
        readonly InstanceUpdate probe;
        readonly BotSettings botSettings;
        bool readyForCommands;

        IDependencyContainer dependencyContainer;

        public IntPtr processPointer;

        private Socket _socket;
        private DispatcherTimer _timer;

        public MainViewModel()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var botSettingsFilePath = Path.Combine(currentFolder, "botSettings.json");
            botSettings = JsonConvert.DeserializeObject<BotSettings>(File.ReadAllText(botSettingsFilePath));
            UpdatePropertiesWithAttribute(typeof(BotSettingAttribute));

            Logger.Initialize(botSettings);
            SqliteRepository.Initialize();
            DiscordClientWrapper.Initialize(botSettings);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _socket.Connect(IPAddress.Parse(botSettings.ListenAddress), botSettings.Port);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }            

            void callback()
            {
                UpdatePropertiesWithAttribute(typeof(ProbeFieldAttribute));
            }
            void killswitch()
            {
                Stop();
            }
            probe = new InstanceUpdate(callback, killswitch)
            {

            };

            InitializeObjectManager();
            
            _timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 500)
            };
            _timer.Tick += (sender, ea) =>
            {
                try
                {
                    OnPropertyChanged(nameof(StartCommandEnabled));
                    OnPropertyChanged(nameof(StopCommandEnabled));

                    if (probe != null)
                    {
                        SendSocketMessage();
                    }
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            };
            _timer.Start();
        }
        public ObservableCollection<IBot> Bots { get; private set; }

        #region Commands

        // Start command
        ICommand startCommand;

        void UiStart()
        {
            Start();
        }
        void StopCallback()
        {
            Stop();
        }

        void Start()
        {
            try
            {
                Bots = new ObservableCollection<IBot>(botLoader.ReloadBots());

                CurrentBot = Bots.First(b => b.Name == "Fury Warrior");

                dependencyContainer = CurrentBot.GetDependencyContainer(botSettings, probe);
                dependencyContainer.AccountName = "OrWr1";

                currentBot.Start(dependencyContainer, StopCallback);

                OnPropertyChanged(nameof(StartCommandEnabled));
                OnPropertyChanged(nameof(StopCommandEnabled));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public ICommand StartCommand =>
            startCommand ?? (startCommand = new CommandHandler(UiStart, true));

        // Stop command
        ICommand stopCommand;

        void UiStop()
        {
            Stop();
        }

        void Stop()
        {
            try
            {
                CurrentBot.Stop();
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        public ICommand StopCommand =>
            stopCommand ?? (stopCommand = new CommandHandler(UiStop, true));

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region Observables
        public bool StartCommandEnabled => !currentBot.Running();

        public bool StopCommandEnabled => currentBot.Running();

        // General
        IBot currentBot;
        public IBot CurrentBot
        {
            get => currentBot;
            set
            {
                currentBot = value;
            }
        }

        #endregion

        private void SendSocketMessage()
        {
            try
            {
                if (_socket == null)
                {
                    return;
                }
                if (!_socket.Connected)
                {
                    try
                    {
                        _socket.Connect(IPAddress.Parse(botSettings.ListenAddress), botSettings.Port);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e.Message);
                    }
                }
                if (_socket != null && _socket.Connected)
                {
                    _socket.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(probe)));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
        }

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