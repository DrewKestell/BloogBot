using Newtonsoft.Json;
using RaidLeaderBot.Pathfinding;
using RaidMemberBot;
using RaidMemberBot.Models;
using RaidMemberBot.Models.Dto;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static RaidLeaderBot.WinImports;

namespace RaidLeaderBot
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly CommandSockerServer _socketServer;
        private readonly RaidLeaderBotSettings _raidLeaderBotSettings;

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

        private static readonly Dictionary<string, Tuple<int, int>> _INSTANCE_ENTRANCE_IDS = new Dictionary<string, Tuple<int, int>>() {
            { _DUNGEON_RAGEFIRE_CHASM, Tuple.Create(389, 2226) }
        };

        private static readonly Regex _regex = new Regex("[0-9]+");

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

        public MainWindow(CommandSockerServer socketServer, RaidLeaderBotSettings raidLeaderBotSettings)
        {
            InitializeComponent();

            SqliteRepository.Initialize();

            _raidLeaderBotSettings = raidLeaderBotSettings;
            _socketServer = socketServer;
            _socketServer.InstanceUpdateObservable.Subscribe(OnInstanceUpdate);

            DataContext = this;

            for (int i = 0; i < InstanceComboBox.Items.Count; i++)
            {
                if (((ComboBoxItem)InstanceComboBox.Items.GetItemAt(i)).Content.ToString() == Activity)
                {
                    InstanceComboBox.SelectedIndex = i;
                    break;
                }
            }

            if (_raidLeaderBotSettings.ActivityPresets.ContainsKey(Activity))
            {
                while (_raidLeaderBotSettings.ActivityPresets[Activity].Count < PresetSelectorComboBox.Items.Count)
                {
                    _raidLeaderBotSettings.ActivityPresets[Activity].Add(new List<RaidMemberPreset>() { new RaidMemberPreset() });
                }
                RaidMemberPresets = _raidLeaderBotSettings.ActivityPresets[Activity][PresetSelectorComboBox.SelectedIndex < 0 ? 0 : PresetSelectorComboBox.SelectedIndex];
            }
            else
            {
                _raidLeaderBotSettings.ActivityPresets.Add(Activity, new List<List<RaidMemberPreset>>() { new List<RaidMemberPreset>() { new RaidMemberPreset() } });
                RaidMemberPresets = _raidLeaderBotSettings.ActivityPresets[Activity][PresetSelectorComboBox.SelectedIndex < 0 ? 0 : PresetSelectorComboBox.SelectedIndex];
            }

            PresetSelectorComboBox.Items.Clear();

            for (int i = 0; i < RaidMemberPresets.Count; i++)
            {
                PresetSelectorComboBox.Items.Add((i + 1).ToString());
            }

            PresetSelectorComboBox.SelectedIndex = 0;

            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanStop));

            UpdateBotLabels();

            Console.WriteLine($"Loading navigation tiles...");
            Navigation.Instance.CalculatePath(1, new Objects.Location(), new Objects.Location(), true);
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
            for (int i = 0; i < RaidMemberPresets.Count; i++)
            {
                if (_characterStates[i].ProcessId == characterState.ProcessId)
                {
                    _characterStates[i] = characterState;

                    if (!_characterStates[i].IsConnected)
                    {
                        _characterStates[i].ProcessId = 0;
                    }

                    CheckForCommand(i, characterState);
                    return;
                }
            }
            for (int i = 0; i < RaidMemberPresets.Count; i++)
            {
                if (_characterStates[i].ProcessId == 0
                    && RaidMemberPresets[i].AccountName == _characterStates[i].AccountName
                    && RaidMemberPresets[i].CharacterSlot == _characterStates[i].CharacterSlot
                    && RaidMemberPresets[i].BotProfileName == _characterStates[i].BotProfileName)
                {
                    _characterStates[i] = characterState;

                    CheckForCommand(i, characterState);
                    return;
                }
            }
            for (int i = 0; i < RaidMemberPresets.Count; i++)
            {
                if (_characterStates[i].ProcessId == 0)
                {
                    _characterStates[i] = characterState;

                    CheckForCommand(i, characterState);
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

                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
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

                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
            }
            catch
            {
                Console.WriteLine("Error encountered starting bot.");
            }
        }
        private void CheckForCommand(int index, CharacterState characterState)
        {
            if (Players[index].ShouldRun)
            {
                if (characterState.ProcessId > 0)
                {
                    if (characterState.Guid == 0
                            || characterState.AccountName != RaidMemberPresets[index].AccountName
                            || characterState.CharacterSlot != RaidMemberPresets[index].CharacterSlot
                            || characterState.BotProfileName != RaidMemberPresets[index].BotProfileName)
                    {
                        var loginCommand = new InstanceCommand()
                        {
                            CommandAction = CommandAction.SetAccountInfo,
                            CommandParam1 = RaidMemberPresets[index].AccountName,
                            CommandParam2 = RaidMemberPresets[index].CharacterSlot.ToString(),
                            CommandParam3 = RaidMemberPresets[index].BotProfileName
                        };

                        _socketServer.SendCommandToProcess(characterState.ProcessId, loginCommand);
                        return;

                    }
                    else if (string.IsNullOrEmpty(characterState.CurrentActivity))
                    {
                        var setActivityCommand = new InstanceCommand()
                        {
                            CommandAction = CommandAction.SetActivity,
                            CommandParam1 = Activity,
                            CommandParam2 = _INSTANCE_ENTRANCE_IDS[Activity].Item1.ToString()
                        };

                        _socketServer.SendCommandToProcess(characterState.ProcessId, setActivityCommand);
                        return;
                    }
                    else if (RaidLeader == null || RaidLeader.CharacterName != characterState.RaidLeader)
                    {
                        if (RaidLeader == null || RaidLeader.ProcessId == 0)
                        {
                            RaidLeader = characterState;
                        }

                        var setLeaderCommand = new InstanceCommand()
                        {
                            CommandAction = CommandAction.SetRaidLeader,
                            CommandParam1 = RaidLeader.CharacterName,
                        };

                        _socketServer.SendCommandToProcess(characterState.ProcessId, setLeaderCommand);
                        return;
                    }
                    else if (RaidLeader.CharacterName != characterState.RaidLeader && !characterState.InParty)
                    {
                        var addPartyMember = new InstanceCommand()
                        {
                            CommandAction = CommandAction.AddPartyMember,
                            CommandParam1 = characterState.CharacterName,
                        };

                        _socketServer.SendCommandToProcess(RaidLeader.ProcessId, addPartyMember);
                        return;
                    }
                    else if (PartyMembers.All(x => x.InParty))
                    {
                        if (PartyMembers.All(x => x.MapId == _INSTANCE_ENTRANCE_IDS[Activity].Item1))
                        {

                            var beginDungeon = new InstanceCommand()
                            {
                                CommandAction = CommandAction.BeginDungeon,
                            };

                            _socketServer.SendCommandToProcess(characterState.ProcessId, beginDungeon);
                            return;
                        }
                        else
                        {
                            if (characterState.MapId != _INSTANCE_ENTRANCE_IDS[Activity].Item1)
                            {
                                AreaTriggerTeleport areaTriggerTeleport = SqliteRepository.GetAreaTriggerTeleportById(_INSTANCE_ENTRANCE_IDS[Activity].Item2);
                                var goToCommand = new InstanceCommand()
                                {
                                    CommandAction = CommandAction.GoTo,
                                    CommandParam1 = areaTriggerTeleport.TargetPositionX.ToString(),
                                    CommandParam2 = areaTriggerTeleport.TargetPositionY.ToString(),
                                    CommandParam3 = areaTriggerTeleport.TargetPositionZ.ToString(),
                                };

                                _socketServer.SendCommandToProcess(characterState.ProcessId, goToCommand);
                                return;
                            }
                        }
                    }
                }
            }
            _socketServer.SendCommandToProcess(characterState.ProcessId, new InstanceCommand());
        }

        public ObservableCollection<PlayerViewModel> Players { get; set; } = new ObservableCollection<PlayerViewModel>();

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private List<RaidMemberPreset> _partyMemberPresets = new List<RaidMemberPreset>();
        public List<RaidMemberPreset> RaidMemberPresets
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
                return !Players.All(x => x.ShouldRun);
            }
        }
        public bool CanStop
        {
            get
            {
                return Players.Any(x => x.ShouldRun);
            }
        }
        public List<CharacterState> PartyMembers
        {
            get
            {
                CharacterState[] relevantCharacters = new CharacterState[Players.Count];
                Array.Copy(_characterStates, relevantCharacters, Players.Count);
                return relevantCharacters.ToList();
            }
        }
        public string Activity
        {
            get
            {
                return _raidLeaderBotSettings.Activity;
            }
            set
            {
                _raidLeaderBotSettings.Activity = value;

                PresetSelectorComboBox.Items.Clear();
                Players.Clear();

                for (int i = 0; i < RaidMemberPresets.Count; i++)
                {
                    PresetSelectorComboBox.Items.Add((i + 1).ToString());
                }

                PresetSelectorComboBox.SelectedIndex = 0;

                RegenerateRaidList();
            }
        }
        public CharacterState RaidLeader { set; get; }
        public bool ShouldRaid
        {
            get
            {
                return _raidLeaderBotSettings.ShouldRaid;
            }
            set
            {
                _raidLeaderBotSettings.ShouldRaid = value;
                OnPropertyChanged(nameof(ShouldRaid));
            }
        }
        public string RaidSize
        {
            get
            {
                return RaidMemberPresets.Count.ToString();
            }
            set
            {
                if (_raidLeaderBotSettings != null)
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
                        int difference = partySize - RaidMemberPresets.Count;

                        if (difference > 0)
                        {
                            for (int i = 0; i < difference; i++)
                            {
                                RaidMemberPresets.Add(new RaidMemberPreset());
                            }
                        }
                        else if (difference < 0)
                        {
                            while (RaidMemberPresets.Count > partySize)
                            {
                                RaidMemberPresets.Remove(RaidMemberPresets.ElementAt(RaidMemberPresets.Count - 1));
                            }
                        }

                        RegenerateRaidList();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
        private void RegenerateRaidList()
        {
            for (int i = Players.Count; i < RaidMemberPresets.Count; i++)
            {
                PlayerViewModel playerViewModel = new PlayerViewModel
                {
                    RaidMemberPreference = RaidMemberPresets[i],
                    ProcessId = "Not Connected",
                    AccountName = RaidMemberPresets[i].AccountName,
                    BotProfileName = RaidMemberPresets[i].BotProfileName,
                    CharacterSlot = RaidMemberPresets[i].CharacterSlot.ToString(),
                    Header = string.Format($"Player {i + 1}")
                };

                Players.Add(playerViewModel);
            }

            while (Players.Count > RaidMemberPresets.Count)
            {
                Players.RemoveAt(Players.Count - 1);
            }

            OnPropertyChanged(nameof(Players));
            OnPropertyChanged(nameof(RaidSize));
        }

        private void UpdateBotLabels()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                CharacterState characterState = _characterStates[i];

                if (characterState != null)
                {
                    Players[i].ProcessId = characterState.ProcessId == 0 ? "Not Connected" : string.Format("Process Id - {0}", characterState.ProcessId.ToString());
                    Players[i].Zone = characterState.Zone;
                    Players[i].Location = characterState.Location;
                    Players[i].CurrentTask = characterState.CurrentTask;
                    Players[i].CurrentActivity = string.IsNullOrEmpty(characterState.CurrentActivity) ? "None" : characterState.CurrentActivity;
                    Players[i].Header = string.IsNullOrEmpty(characterState.CharacterName)
                                        ? string.Format($"Player {i + 1}")
                                        : string.Format($"Player {i + 1} - {characterState.CharacterName}");
                }
            }

            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanStop));

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
        private void RaidSizeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (_regex.IsMatch(RaidSizeTextBox.Text))
            {
                try
                {
                    if (!string.IsNullOrEmpty(RaidSizeTextBox.Text))
                    {
                        int requestedRaidSize = int.Parse(RaidSizeTextBox.Text);
                        if (requestedRaidSize <= 0)
                        {
                            RaidSizeTextBox.Text = "1";
                            requestedRaidSize = 1;
                        }
                        else if (requestedRaidSize > 40)
                        {
                            RaidSizeTextBox.Text = "40";
                            requestedRaidSize = 40;
                        }

                        e.Handled = requestedRaidSize < 1 || requestedRaidSize > 40;
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
            if (_raidLeaderBotSettings != null && sender is ComboBox comboBox && comboBox.IsEnabled)
            {
                Activity = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
            }
        }

        private void AddPresetButton_Click(object sender, RoutedEventArgs e)
        {
            PresetSelectorComboBox.Items.Add((PresetSelectorComboBox.Items.Count + 1).ToString());
            PresetSelectorComboBox.SelectedIndex = PresetSelectorComboBox.Items.Count - 1;

            AddPresetButton.IsEnabled = _raidLeaderBotSettings.ActivityPresets[Activity].Count < 10;
            RemovePresetButton.IsEnabled = _raidLeaderBotSettings.ActivityPresets[Activity].Count > 1;
        }

        private void RemovePresetButton_Click(object sender, RoutedEventArgs e)
        {
            _raidLeaderBotSettings.ActivityPresets[Activity].RemoveAt(PresetSelectorComboBox.SelectedIndex);
            PresetSelectorComboBox.Items.RemoveAt(PresetSelectorComboBox.Items.Count - 1);

            if (PresetSelectorComboBox.SelectedIndex < 0)
            {
                PresetSelectorComboBox.SelectedIndex = PresetSelectorComboBox.Items.Count - 1;
            }

            AddPresetButton.IsEnabled = _raidLeaderBotSettings.ActivityPresets[Activity].Count < 10;
            RemovePresetButton.IsEnabled = _raidLeaderBotSettings.ActivityPresets[Activity].Count > 1;

            RegenerateRaidList();
        }

        private void PresetSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_raidLeaderBotSettings != null && sender is ComboBox comboBox && comboBox.SelectedIndex > -1)
            {
                if (_raidLeaderBotSettings.ActivityPresets.ContainsKey(Activity))
                {
                    while (_raidLeaderBotSettings.ActivityPresets[Activity].Count < PresetSelectorComboBox.Items.Count)
                    {
                        _raidLeaderBotSettings.ActivityPresets[Activity].Add(new List<RaidMemberPreset>() { new RaidMemberPreset() });
                    }
                    RaidMemberPresets = _raidLeaderBotSettings.ActivityPresets[Activity][PresetSelectorComboBox.SelectedIndex < 0 ? 0 : PresetSelectorComboBox.SelectedIndex];
                }
                else
                {
                    _raidLeaderBotSettings.ActivityPresets.Add(Activity, new List<List<RaidMemberPreset>>() { new List<RaidMemberPreset>() { new RaidMemberPreset() } });
                    RaidMemberPresets = _raidLeaderBotSettings.ActivityPresets[Activity][PresetSelectorComboBox.SelectedIndex < 0 ? 0 : PresetSelectorComboBox.SelectedIndex];
                }

                Players.Clear();
                RegenerateRaidList();
            }
        }
        private void SaveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _raidLeaderBotSettings.ActivityPresets.ToList().ForEach(activity =>
                {
                    activity.Value.ForEach(presetList =>
                    {
                        presetList.RemoveAll(partyMemberPreset => string.IsNullOrEmpty(partyMemberPreset.AccountName)
                                                || string.IsNullOrEmpty(partyMemberPreset.BotProfileName)
                                                || partyMemberPreset.CharacterSlot == 0);
                    });
                });

                _raidLeaderBotSettings.ActivityPresets.ToList().RemoveAll(x => x.Value.Count == 0);

                var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var botSettingsFilePath = Path.Combine(currentFolder, "Settings\\raidLeaderBotSettings.json");
                var json = JsonConvert.SerializeObject(_raidLeaderBotSettings, Formatting.Indented);
                File.WriteAllText(botSettingsFilePath, json);

                Console.WriteLine("Settings successfully saved!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private void LaunchProcess()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var startupInfo = new STARTUPINFO();
            // run BloogBot.exe in a new process
            CreateProcess(
                _raidLeaderBotSettings.PathToWoW,
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
            get => RaidMemberPreference.AccountName;
            set
            {
                RaidMemberPreference.AccountName = value;
            }
        }

        public string CharacterSlot
        {
            get => RaidMemberPreference.CharacterSlot.ToString();
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

                    RaidMemberPreference.CharacterSlot = slot;
                }
                catch (Exception)
                {

                }
            }
        }

        public string BotProfileName
        {
            get => RaidMemberPreference.BotProfileName;
            set
            {
                RaidMemberPreference.BotProfileName = value;
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

        private string _location;

        public string Location
        {
            get => _location;
            set
            {
                _location = value;
                OnPropertyChanged(nameof(Location));
            }
        }

        public RaidMemberPreset RaidMemberPreference;

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