using BloogBot;
using BloogBot.Models.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static Bootstrapper.WinImports;

namespace Bootstrapper
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly SocketServer _socketServer;
        private readonly BootstrapperSettings _bootstrapperSettings;

        private static readonly string _PVP_WARSONG_GULTCH = "10 Warsong Gultch";
        private static readonly string _PVP_ARATHI_BASIN = "15 Arathi Basin";
        private static readonly string _PVP_ALTERAC_VALLEY = "40 Alterac Valley";

        private static readonly string _DUNGEON_RAGEFIRE_CHASM = "10 [ Ragefire Chasm (13-18)";
        private static readonly string _DUNGEON_WAILING_CAVERNS = "10 Wailing Caverns (15-25)";
        private static readonly string _DUNGEON_THE_DEADMINES = "10 The Deadmines (18-23)";
        private static readonly string _DUNGEON_SHADOWFANG_KEEP = "10 Shadowfang Keep (22-30)";
        private static readonly string _DUNGEON_THE_STOKADE = "10 ] The Stockade (22-30)";
        private static readonly string _DUNGEON_BLACKFATHOM_DEEPS = "10 Blackfathom Deeps (24-32)";
        private static readonly string _DUNGEON_GNOMEREGAN = "10 Gnomeregan (29-38)";
        private static readonly string _DUNGEON_RAZORFEN_KRAUL = "10 Razorfen Kraul (30-40)";
        private static readonly string _DUNGEON_SM_GRAVEYARD = "10 The Scarlet Monastery - Graveyard (28-38)";
        private static readonly string _DUNGEON_SM_LIBRARY = "10 The Scarlet Monastery - Library (29-39)";
        private static readonly string _DUNGEON_SM_ARMORY = "10 The Scarlet Monastery - Armory (32-42)";
        private static readonly string _DUNGEON_SM_CATHEDRAL = "10 The Scarlet Monastery - Cathedral (35-45)";
        private static readonly string _DUNGEON_RAZORFEN_DOWNS = "10 Razorfen Downs (40-50)";
        private static readonly string _DUNGEON_ULDAMAN = "10 Uldaman (42-52)";
        private static readonly string _DUNGEON_ZULFARRAK = "10 Zul'Farrak (42-46)";
        private static readonly string _DUNGEON_WICKED_GROTTO = "10 Maraudon - Wicked Grotto (Purple) (46-55)";
        private static readonly string _DUNGEON_FOULSPORE_CAVERN = "10 Maraudon - Foulspore Cavern (Orange) (46-55)";
        private static readonly string _DUNGEON_EARTH_SONG_FALLS = "10 Maraudon - Earth Song Falls (Inner) (46-55)";
        private static readonly string _DUNGEON_TEMPLE_OF_ATALHAKKAR = "10 Temple of Atal'Hakkar (50-56)";
        private static readonly string _DUNGEON_BLACKROCK_DEPTHS = "40 Blackrock Depths (52-60)";
        private static readonly string _DUNGEON_LOWER_BLACKROCK_SPIRE = "10 Lower Blackrock Spire (55-60)";
        private static readonly string _DUNGEON_UPPER_BLACKROCK_SPIRE = "10 Upper Blackrock Spire (58-60)";
        private static readonly string _DUNGEON_DIRE_MAUL = "5 Dire Maul (55-60)";
        private static readonly string _DUNGEON_STRATHOLME = "5 Stratholme (58-60)";
        private static readonly string _DUNGEON_SCHOLOMANCE = "5 Scholomance (58-60)";
        private static readonly string _DUNGEON_ONYXIAS_LAIR = "40 Onyxia's Lair (60+)";
        private static readonly string _DUNGEON_ZUL_GURUB = "20 Zul'Gurub (60+)";
        private static readonly string _DUNGEON_MOLTEN_CORE = "40 Molten Core (60+)";
        private static readonly string _DUNGEON_BLACKWING_LAIR = "40 Blackwing Lair (60++)";
        private static readonly string _DUNGEON_RUINS_OF_AHNQIRAJ = "20 Ruins of Ahn'Qiraj (60++)";
        private static readonly string _DUNGEON_TEMPLE_OF_AHNQIRAJ = "40 Temple of Ahn'Qiraj (60+++)";
        private static readonly string _DUNGEON_NAXXRAMAS = "40 Naxxramas (60++++)";

        private static readonly Regex _regex = new Regex("[0-9]+");

        private Task _backgroundTask;

        public readonly CharacterState[] _characterStates = {
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
        };

        public MainWindow(SocketServer socketServer, BootstrapperSettings bootstrapperSettings)
        {
            InitializeComponent();

            _bootstrapperSettings = bootstrapperSettings;
            _socketServer = socketServer;
            _socketServer.InstanceUpdateObservable.Subscribe(OnInstanceUpdate);

            DataContext = this;

            if (Activity == "Questing")
            {
                QuestingRadioButton.IsChecked = true;
                PartyCheckBox.IsEnabled = true;
            }
            else
            {
                InstanceRadioButton.IsChecked = true;
                PartyCheckBox.IsEnabled = false;
                PartyCheckBox.IsChecked = true;
            }

            for (int i = 0; i < InstanceComboBox.Items.Count; i++)
            {
                if (((ComboBoxItem)InstanceComboBox.Items.GetItemAt(i)).Content.ToString() == Activity)
                {
                    InstanceComboBox.SelectedIndex = i;
                    break;
                }
            }

            if (_bootstrapperSettings.ActivityPresets.ContainsKey(Activity))
            {
                while (_bootstrapperSettings.ActivityPresets[Activity].Count < PresetSelectorComboBox.Items.Count)
                {
                    _bootstrapperSettings.ActivityPresets[Activity].Add(new List<PartyMemberPreset>() { new PartyMemberPreset() });
                }
                PartyMemberPresets = _bootstrapperSettings.ActivityPresets[Activity][PresetSelectorComboBox.SelectedIndex < 0 ? 0 : PresetSelectorComboBox.SelectedIndex];
            }
            else
            {
                _bootstrapperSettings.ActivityPresets.Add(Activity, new List<List<PartyMemberPreset>>() { new List<PartyMemberPreset>() { new PartyMemberPreset() } });
                PartyMemberPresets = _bootstrapperSettings.ActivityPresets[Activity][PresetSelectorComboBox.SelectedIndex < 0 ? 0 : PresetSelectorComboBox.SelectedIndex];
            }

            PresetSelectorComboBox.Items.Clear();

            for (int i = 0; i < PartyMemberPresets.Count; i++)
            {
                PresetSelectorComboBox.Items.Add((i + 1).ToString());
            }

            PresetSelectorComboBox.SelectedIndex = 0;

            UpdateBotLabels();

            _backgroundTask = Task.Run(StartSync);

            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanStop));
        }
        private void OnInstanceUpdate(CharacterState update)
        {
            Dispatcher.Invoke(() =>
            {
                ConsumeMessage(update);
                UpdateBotLabels();
            });
        }
        public void ConsumeMessage(CharacterState characterState)
        {
            for (int i = 0; i < PartyMemberPresets.Count; i++)
            {
                if (_characterStates[i].ProcessId == characterState.ProcessId)
                {
                    _characterStates[i] = characterState;

                    if (!_characterStates[i].IsConnected)
                    {
                        _characterStates[i].ProcessId = 0;
                    }

                    return;
                }
            }
            for (int i = 0; i < PartyMemberPresets.Count; i++)
            {
                if (_characterStates[i].ProcessId == 0
                    && PartyMemberPresets[i].AccountName == _characterStates[i].AccountName
                    && PartyMemberPresets[i].CharacterSlot == _characterStates[i].CharacterSlot
                    && PartyMemberPresets[i].BotProfileName == _characterStates[i].BotProfileName)
                {
                    _characterStates[i] = characterState;
                    return;
                }
            }
            for (int i = 0; i < PartyMemberPresets.Count; i++)
            {
                if (_characterStates[i].ProcessId == 0)
                {
                    _characterStates[i] = characterState;
                    return;
                }
            }
        }
        private void StartAll()
        {
            try
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    Players[i].ShouldRun = true;
                }

                StartAllBotsButton.IsEnabled = false;
            }
            catch
            {
                Console.WriteLine("Error encountered starting bot.");
            }
        }
        private void StopAll()
        {
            try
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    Players[i].ShouldRun = false;
                }

                StopAllBotsButton.IsEnabled = false;
            }
            catch
            {
                Console.WriteLine("Error encountered starting bot.");
            }
        }
        private async Task StartSync()
        {
            while (true)
            {
                for (int i = 0; i < PartyMemberPresets.Count; i++)
                {
                    if (Players[i].ShouldRun)
                    {
                        if (_characterStates[i].ProcessId > 0)
                        {
                            if (_characterStates[i].IsRunning)
                            {
                                if (_characterStates[i].Guid == 0
                                        || _characterStates[i].AccountName != PartyMemberPresets[i].AccountName
                                        || _characterStates[i].CharacterSlot != PartyMemberPresets[i].CharacterSlot
                                        || _characterStates[i].BotProfileName != PartyMemberPresets[i].BotProfileName)
                                {
                                    if (!_characterStates[i].SetAccountInfoRequested)
                                    {
                                        var loginCommand = new InstanceCommand()
                                        {
                                            CommandName = InstanceCommand.SET_ACCOUNT_INFO,
                                            CommandParam1 = PartyMemberPresets[i].AccountName,
                                            CommandParam2 = PartyMemberPresets[i].CharacterSlot.ToString(),
                                            CommandParam3 = PartyMemberPresets[i].BotProfileName
                                        };

                                        _socketServer.SendCommandToProcess(_characterStates[i].ProcessId, loginCommand);
                                        _characterStates[i].SetAccountInfoRequested = true;
                                    }
                                }
                            }
                            else
                            {
                                if (!_characterStates[i].StartRequested)
                                {
                                    var startCommand = new InstanceCommand()
                                    {
                                        CommandName = InstanceCommand.START,
                                    };

                                    _socketServer.SendCommandToProcess(_characterStates[i].ProcessId, startCommand);
                                    _characterStates[i].StartRequested = true;
                                }
                            }
                        }
                        else
                        {
                            LaunchProcess();
                        }
                    }
                    else
                    {
                        if (_characterStates[i].ProcessId > 0 && _characterStates[i].IsRunning && !_characterStates[i].StopRequested)
                        {
                            var stopCommand = new InstanceCommand()
                            {
                                CommandName = InstanceCommand.STOP,
                            };

                            _socketServer.SendCommandToProcess(_characterStates[i].ProcessId, stopCommand);
                            _characterStates[i].StopRequested = true;
                        }
                    }
                }

                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));

                await Task.Delay(100);
            }
        }

        public ObservableCollection<PlayerViewModel> Players { get; set; } = new ObservableCollection<PlayerViewModel>();

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private List<PartyMemberPreset> _partyMemberPresets = new List<PartyMemberPreset>();
        public List<PartyMemberPreset> PartyMemberPresets
        {
            get
            {
                return _partyMemberPresets;
            }
            set
            {
                _partyMemberPresets = value;
            }
        }
        public bool CanStart
        {
            get
            {
                return !Players.All(x => x.IsRunning);
            }
        }
        public bool CanStop
        {
            get
            {
                return Players.Any(x => x.IsRunning);
            }
        }
        public string Activity
        {
            get
            {
                return _bootstrapperSettings.Activity;
            }
            set
            {
                _bootstrapperSettings.Activity = value;

                PresetSelectorComboBox.Items.Clear();
                Players.Clear();

                for (int i = 0; i < PartyMemberPresets.Count; i++)
                {
                    PresetSelectorComboBox.Items.Add((i + 1).ToString());
                }

                PresetSelectorComboBox.SelectedIndex = 0;

                RegeneratePartyList();
            }
        }
        public bool ShouldParty
        {
            get
            {
                return _bootstrapperSettings.ShouldParty;
            }
            set
            {
                _bootstrapperSettings.ShouldParty = value;
                OnPropertyChanged(nameof(ShouldParty));
            }
        }
        public string PartySize
        {
            get
            {
                return PartyMemberPresets.Count.ToString();
            }
            set
            {
                if (_bootstrapperSettings != null)
                {
                    try
                    {
                        int partySize = int.Parse(value);

                        if (partySize < 1)
                        {
                            partySize = 1;
                        }
                        else if (partySize > 40)
                        {
                            partySize = 40;
                        }
                        int difference = partySize - PartyMemberPresets.Count;

                        if (difference > 0)
                        {
                            for (int i = 0; i < difference; i++)
                            {
                                PartyMemberPresets.Add(new PartyMemberPreset());
                            }
                        }
                        else if (difference < 0)
                        {
                            while (PartyMemberPresets.Count > partySize)
                            {
                                PartyMemberPresets.Remove(PartyMemberPresets.ElementAt(PartyMemberPresets.Count - 1));
                            }
                        }

                        RegeneratePartyList();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
        private void RegeneratePartyList()
        {
            for (int i = Players.Count; i < PartyMemberPresets.Count; i++)
            {
                PlayerViewModel playerViewModel = new PlayerViewModel
                {
                    PartyMemberPreference = PartyMemberPresets[i],
                    ProcessId = "Not Connected",
                    AccountName = PartyMemberPresets[i].AccountName,
                    BotProfileName = PartyMemberPresets[i].BotProfileName,
                    CharacterSlot = PartyMemberPresets[i].CharacterSlot.ToString(),
                };

                Players.Add(playerViewModel);
            }

            while (Players.Count > PartyMemberPresets.Count)
            {
                Players.RemoveAt(Players.Count - 1);
            }

            OnPropertyChanged(nameof(Players));
            OnPropertyChanged(nameof(PartySize));
        }

        private void UpdateBotLabels()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                CharacterState characterState = _characterStates[i];

                if (characterState != null)
                {
                    Players[i].ProcessId = characterState.ProcessId == 0 ? "Not Connected" : string.Format("Process Id - {0}", characterState.ProcessId.ToString());
                    Players[i].IsRunning = characterState.IsRunning;
                    Players[i].Zone = characterState.Zone;
                    Players[i].Position = characterState.Position == null ? string.Empty : characterState.Position.ToString();
                    Players[i].CurrentTask = string.IsNullOrEmpty(characterState.CurrentTask) ? "Idle" : characterState.CurrentTask;
                    Players[i].CurrentActivity = string.IsNullOrEmpty(characterState.CurrentActivity) ? "None" : characterState.CurrentActivity;
                    Players[i].Header = string.IsNullOrEmpty(characterState.CharacterName)
                                        ? string.Format($"Player {i + 1}")
                                        : string.Format($"Player {i + 1} - {characterState.CharacterName}");
                }
            }
        }
        private void NewWoWLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            LaunchProcess();
        }
        private void AllBotsControlButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button controlButton)
            {
                if (controlButton == StartAllBotsButton)
                {
                    StartAll();
                }
                else
                {
                    StopAll();
                }
            }
        }
        private void ActivityRadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.IsChecked == true)
            {
                if (radioButton == QuestingRadioButton)
                {
                    Activity = "Questing";
                    PartyCheckBox.IsEnabled = true;
                }
                else
                {
                    PartyCheckBox.IsEnabled = false;
                    PartyCheckBox.IsChecked = true;

                    Activity = ((ComboBoxItem)InstanceComboBox.SelectedItem).Content.ToString();
                }
            }
        }
        private void PartySizeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (_regex.IsMatch(PartySizeTextBox.Text))
            {
                try
                {
                    if (!string.IsNullOrEmpty(PartySizeTextBox.Text))
                    {
                        int requestedPartySize = int.Parse(PartySizeTextBox.Text);
                        if (requestedPartySize <= 0)
                        {
                            PartySizeTextBox.Text = "1";
                            requestedPartySize = 1;
                        }
                        else if (requestedPartySize > 40)
                        {
                            PartySizeTextBox.Text = "40";
                            requestedPartySize = 40;
                        }

                        e.Handled = requestedPartySize < 1 || requestedPartySize > 40;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Preview " + ex.Message);
                }
            }
        }

        private void InstanceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_bootstrapperSettings != null && sender is ComboBox comboBox && comboBox.IsEnabled)
            {
                Activity = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
            }
        }

        private void AddPresetButton_Click(object sender, RoutedEventArgs e)
        {
            PresetSelectorComboBox.Items.Add((PresetSelectorComboBox.Items.Count + 1).ToString());
            PresetSelectorComboBox.SelectedIndex = PresetSelectorComboBox.Items.Count - 1;

            AddPresetButton.IsEnabled = _bootstrapperSettings.ActivityPresets[Activity].Count < 10;
            RemovePresetButton.IsEnabled = _bootstrapperSettings.ActivityPresets[Activity].Count > 1;
        }

        private void RemovePresetButton_Click(object sender, RoutedEventArgs e)
        {
            _bootstrapperSettings.ActivityPresets[Activity].RemoveAt(PresetSelectorComboBox.SelectedIndex);
            PresetSelectorComboBox.Items.RemoveAt(PresetSelectorComboBox.Items.Count - 1);

            if (PresetSelectorComboBox.SelectedIndex < 0)
            {
                PresetSelectorComboBox.SelectedIndex = PresetSelectorComboBox.Items.Count - 1;
            }

            AddPresetButton.IsEnabled = _bootstrapperSettings.ActivityPresets[Activity].Count < 10;
            RemovePresetButton.IsEnabled = _bootstrapperSettings.ActivityPresets[Activity].Count > 1;

            RegeneratePartyList();
        }

        private void PresetSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_bootstrapperSettings != null && sender is ComboBox comboBox && comboBox.SelectedIndex > -1)
            {
                if (_bootstrapperSettings.ActivityPresets.ContainsKey(Activity))
                {
                    while (_bootstrapperSettings.ActivityPresets[Activity].Count < PresetSelectorComboBox.Items.Count)
                    {
                        _bootstrapperSettings.ActivityPresets[Activity].Add(new List<PartyMemberPreset>() { new PartyMemberPreset() });
                    }
                    PartyMemberPresets = _bootstrapperSettings.ActivityPresets[Activity][PresetSelectorComboBox.SelectedIndex < 0 ? 0 : PresetSelectorComboBox.SelectedIndex];
                }
                else
                {
                    _bootstrapperSettings.ActivityPresets.Add(Activity, new List<List<PartyMemberPreset>>() { new List<PartyMemberPreset>() { new PartyMemberPreset() } });
                    PartyMemberPresets = _bootstrapperSettings.ActivityPresets[Activity][PresetSelectorComboBox.SelectedIndex < 0 ? 0 : PresetSelectorComboBox.SelectedIndex];
                }

                Players.Clear();
                RegeneratePartyList();
            }
        }
        private void SaveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _bootstrapperSettings.ActivityPresets.ToList().ForEach(activity =>
                {
                    activity.Value.ForEach(presetList =>
                    {
                        presetList.RemoveAll(partyMemberPreset => string.IsNullOrEmpty(partyMemberPreset.AccountName)
                                                || string.IsNullOrEmpty(partyMemberPreset.BotProfileName)
                                                || partyMemberPreset.CharacterSlot == 0);
                    });
                });

                _bootstrapperSettings.ActivityPresets.ToList().RemoveAll(x => x.Value.Count == 0);

                var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var botSettingsFilePath = Path.Combine(currentFolder, "bootstrapperSettings.json");
                var json = JsonConvert.SerializeObject(_bootstrapperSettings, Formatting.Indented);
                File.WriteAllText(botSettingsFilePath, json);

                Console.WriteLine("Settings successfully saved!");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
        private void LaunchProcess()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var startupInfo = new STARTUPINFO();
            // run BloogBot.exe in a new process
            CreateProcess(
                _bootstrapperSettings.PathToWoW,
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
                throw new InvalidOperationException(
                    $"Failed to write Loader.dll into the WoW.exe process, error code: {error}");

            // search current process's for the memory address of the LoadLibraryW function within the kernel32.dll module
            var loaderDllPointer = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");

            // this seems to help prevent timing issues
            Thread.Sleep(1000);

            error = Marshal.GetLastWin32Error();
            if (error > 0)
                throw new InvalidOperationException(
                    $"Failed to get memory address to Loader.dll in the WoW.exe process, error code: {error}");

            // create a new thread with the execution starting at the LoadLibraryW function, 
            // with the path to our Loader.dll passed as a parameter
            CreateRemoteThread(processHandle, (IntPtr)null, (IntPtr)0, loaderDllPointer, loaderPathPtr, 0, (IntPtr)null);

            // this seems to help prevent timing issues
            Thread.Sleep(1000);

            error = Marshal.GetLastWin32Error();
            if (error > 0)
                throw new InvalidOperationException(
                    $"Failed to create remote thread to start execution of Loader.dll in the WoW.exe process, error code: {error}");

            // free the memory that was allocated by VirtualAllocEx
            VirtualFreeEx(processHandle, loaderPathPtr, 0, MemoryFreeType.MEM_RELEASE);
        }
    }
    public sealed class PlayerViewModel : INotifyPropertyChanged
    {
        ICommand _startCommand;

        public ICommand StartCommand =>
            _startCommand ?? (_startCommand = new CommandHandler(StartBot, true));

        public void StartBot()
        {
            ShouldRun = true;
        }

        ICommand _stopCommand;

        public ICommand StopCommand =>
            _stopCommand ?? (_stopCommand = new CommandHandler(StopBot, true));

        public void StopBot()
        {
            ShouldRun = false;
        }

        ICommand _updateCommand;

        public ICommand UpdateCommand =>
            _updateCommand ?? (_updateCommand = new CommandHandler(UpdatePlayerPreference, true));

        public void UpdatePlayerPreference()
        {
        }
        private bool _shouldRun;

        public bool ShouldRun
        {
            get => _shouldRun;
            set
            {
                _shouldRun = value;
                OnPropertyChanged(nameof(ShouldRun));
            }
        }
        private bool _isRunning;

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        private string _header;

        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                OnPropertyChanged(nameof(Header));
            }
        }

        public string AccountName
        {
            get => PartyMemberPreference.AccountName;
            set
            {
                PartyMemberPreference.AccountName = value;
            }
        }

        public string CharacterSlot
        {
            get => PartyMemberPreference.CharacterSlot.ToString();
            set
            {
                try
                {
                    int slot = int.Parse(value);

                    if (slot < 1)
                    {
                        slot = 1;
                    }
                    else if (slot > 10)
                    {
                        slot = 10;
                    }

                    PartyMemberPreference.CharacterSlot = slot;
                }
                catch (Exception)
                {

                }
            }
        }

        public string BotProfileName
        {
            get => PartyMemberPreference.BotProfileName;
            set
            {
                PartyMemberPreference.BotProfileName = value;
            }
        }

        private string _zone;

        public string Zone
        {
            get => _zone;
            set
            {
                _zone = value;
                OnPropertyChanged(nameof(Zone));
            }
        }

        private string _processId;

        public string ProcessId
        {
            get => _processId == "0" ? "Not Connected" : _processId;
            set
            {
                _processId = value;
                OnPropertyChanged(nameof(ProcessId));
            }
        }

        private string _currentTask;

        public string CurrentTask
        {
            get => _currentTask;
            set
            {
                _currentTask = value;
                OnPropertyChanged(nameof(CurrentTask));
            }
        }

        private string _currentActivity;

        public string CurrentActivity
        {
            get => _currentActivity;
            set
            {
                _currentActivity = value;
                OnPropertyChanged(nameof(CurrentActivity));
            }
        }

        private string _position;

        public string Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged(nameof(Position));
            }
        }

        public PartyMemberPreset PartyMemberPreference;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}