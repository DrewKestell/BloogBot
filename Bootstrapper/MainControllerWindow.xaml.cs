
using BloogBot.AI;
using BloogBot.Models;
using BloogBot.Models.Dto;
using BloogBot.Models.Enums;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static Bootstrapper.WinImports;

namespace Bootstrapper
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly SocketServer _socketServer;
        private readonly BootstrapperSettings _bootstrapperSettings;

        private static readonly string[] _races = { "Human", "Dwarf", "Night Elf", "Gnome", "Orc", "Undead", "Tauren", "Troll" };
        private static readonly string[] _classes = { "Druid", "Hunter", "Mage", "Paladin", "Priest", "Rogue", "Shaman", "Warlock", "Warrior" };

        private static readonly string[] _humanClasses = { "Mage", "Paladin", "Priest", "Rogue", "Warlock", "Warrior" };
        private static readonly string[] _dwarfClasses = { "Hunter", "Paladin", "Priest", "Rogue", "Warrior" };
        private static readonly string[] _nightElfClasses = { "Druid", "Hunter", "Priest", "Rogue", "Warrior" };
        private static readonly string[] _gnomeClasses = { "Mage", "Rogue", "Warlock", "Warrior" };

        private static readonly string[] _orcClasses = { "Hunter", "Rogue", "Shaman", "Warlock", "Warrior" };
        private static readonly string[] _undeadClasses = { "Mage", "Priest", "Rogue", "Warlock", "Warrior" };
        private static readonly string[] _taurenClasses = { "Druid", "Hunter", "Shaman", "Warrior" };
        private static readonly string[] _trollClasses = { "Hunter", "Mage", "Priest", "Rogue", "Shaman", "Warrior" };

        private static readonly string[] _dungeonInstances = { "Ragefire Chasm", "Wailing Caverns", "Shadowfang Keep",  "The Stockade",
                                                            "Blackfathom Deeps", "Gnomeregan", "Razorfen Kraul",
                                                            "The Scarlet Monastery - Graveyard", "The Scarlet Monastery - Library",
                                                            "The Scarlet Monastery - Armory", "The Scarlet Monastery - Cathedral",
                                                            "Razorfen Downs", "Uldaman", "Zul'Farrak", "Maraudon - Wicked Grotto (Purple)",
                                                            "Maraudon - Foulspore Cavern (Orange)", "Maraudon - Earth Song Falls (Inner)",
                                                            "Temple of Atal'Hakkar", "Blackrock Depths", "Lower Blackrock Spire",
                                                            "Upper Blackrock Spire", "Dire Maul", "Stratholme", "Scholomance", "Onyxia's Lair",
                                                            "Zul'Gurub", "Molten Core", "Blackwing Lair", "Ruins of Ahn'Qiraj",
                                                            "Temple of Ahn'Qiraj", "Naxxramas" };

        private bool _shouldRun;
        private Task _backgroundTask;

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public MainWindow(SocketServer socketServer, BootstrapperSettings bootstrapperSettings)
        {
            InitializeComponent();
            _socketServer = socketServer;
            _socketServer.InstanceUpdateObservable.Subscribe(OnInstanceUpdate);
            _bootstrapperSettings = bootstrapperSettings;

            if (_bootstrapperSettings.Activity == "Dungeon")
            {
                DungeonRadioButton.IsChecked = true;
                PartyCheckBox.IsChecked = true;
                PartyCheckBox.IsEnabled = false;
            }
            else if (_bootstrapperSettings.Activity == "PvP")
            {
                PvPRadioButton.IsChecked = true;
                PartyCheckBox.IsEnabled = false;
            }
            else
            {
                QuestingRadioButton.IsChecked = true;
                PartyCheckBox.IsEnabled = true;
                PartyCheckBox.IsChecked = _bootstrapperSettings.ShouldParty;
            }

            for (int i = 0; i < DungeonComboBox.Items.Count; i++)
            {
                if (DungeonComboBox.Items.GetItemAt(i).ToString() == _bootstrapperSettings.Dungeon)
                {
                    DungeonComboBox.SelectedIndex = i;
                    break;
                }
            }

            if (_bootstrapperSettings.PvPSelection == "Warsong Gultch")
            {
                PvPInstanceComboBox.SelectedIndex = 0;
            }
            else if (_bootstrapperSettings.PvPSelection == "Arathi Basin")
            {
                PvPInstanceComboBox.SelectedIndex = 1;
            }
            else
            {
                PvPInstanceComboBox.SelectedIndex = 2;
            }

            PartySizeTextBox.Text = _bootstrapperSettings.PartySize.ToString();

            for (int i = 0; i < _bootstrapperSettings.PartyPreferences.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        for (int j = 0; j < Player01RaceComboBox.Items.Count; j++)
                        {
                            if (((ComboBoxItem)Player01RaceComboBox.Items.GetItemAt(j)).Content.ToString() == _bootstrapperSettings.PartyPreferences[i].Race.ToString())
                            {
                                Player01RaceComboBox.SelectedIndex = j;
                                break;
                            }
                        }
                        for (int j = 0; j < Player01ClassComboBox.Items.Count; j++)
                        {
                            if (((ComboBoxItem)Player01ClassComboBox.Items.GetItemAt(j)).Content.ToString() == _bootstrapperSettings.PartyPreferences[i].Class.ToString())
                            {
                                AssignClassOptions(i, Player01TankCheckBox, Player01HealerCheckBox);
                                Player01ClassComboBox.SelectedIndex = j;
                                break;
                            }
                        }
                        Player01TankCheckBox.IsChecked = (_bootstrapperSettings.PartyPreferences[i].Role & Role.Tank) == Role.Tank;
                        Player01DamageCheckBox.IsChecked = (_bootstrapperSettings.PartyPreferences[i].Role & Role.Damage) == Role.Damage;
                        Player01HealerCheckBox.IsChecked = (_bootstrapperSettings.PartyPreferences[i].Role & Role.Healer) == Role.Healer;
                        break;
                    case 1:
                        for (int j = 0; j < Player02RaceComboBox.Items.Count; j++)
                        {
                            if (((ComboBoxItem)Player02RaceComboBox.Items.GetItemAt(j)).Content.ToString() == _bootstrapperSettings.PartyPreferences[i].Race.ToString())
                            {
                                Player02RaceComboBox.SelectedIndex = j;
                                break;
                            }
                        }
                        for (int j = 0; j < Player02ClassComboBox.Items.Count; j++)
                        {
                            if (((ComboBoxItem)Player02ClassComboBox.Items.GetItemAt(j)).Content.ToString() == _bootstrapperSettings.PartyPreferences[i].Class.ToString())
                            {
                                AssignClassOptions(i, Player02TankCheckBox, Player02HealerCheckBox);
                                Player02ClassComboBox.SelectedIndex = j;
                                break;
                            }
                        }
                        Player02TankCheckBox.IsChecked = (_bootstrapperSettings.PartyPreferences[i].Role & Role.Tank) == Role.Tank;
                        Player02DamageCheckBox.IsChecked = (_bootstrapperSettings.PartyPreferences[i].Role & Role.Damage) == Role.Damage;
                        Player02HealerCheckBox.IsChecked = (_bootstrapperSettings.PartyPreferences[i].Role & Role.Healer) == Role.Healer;
                        break;
                    case 2:
                        for (int j = 0; j < Player03RaceComboBox.Items.Count; j++)
                        {
                            if (((ComboBoxItem)Player03RaceComboBox.Items.GetItemAt(j)).Content.ToString() == _bootstrapperSettings.PartyPreferences[i].Race.ToString())
                            {
                                Player03RaceComboBox.SelectedIndex = j;
                                break;
                            }
                        }
                        for (int j = 0; j < Player03ClassComboBox.Items.Count; j++)
                        {
                            if (((ComboBoxItem)Player03ClassComboBox.Items.GetItemAt(j)).Content.ToString() == _bootstrapperSettings.PartyPreferences[i].Class.ToString())
                            {
                                AssignClassOptions(i, Player03TankCheckBox, Player03HealerCheckBox);
                                Player03ClassComboBox.SelectedIndex = j;
                                break;
                            }
                        }
                        Player03TankCheckBox.IsChecked = (_bootstrapperSettings.PartyPreferences[i].Role & Role.Tank) == Role.Tank;
                        Player03DamageCheckBox.IsChecked = (_bootstrapperSettings.PartyPreferences[i].Role & Role.Damage) == Role.Damage;
                        Player03HealerCheckBox.IsChecked = (_bootstrapperSettings.PartyPreferences[i].Role & Role.Healer) == Role.Healer;
                        break;
                    case 3:
                        for (int j = 0; j < Player04RaceComboBox.Items.Count; j++)
                        {
                            if (((ComboBoxItem)Player04RaceComboBox.Items.GetItemAt(j)).Content.ToString() == _bootstrapperSettings.PartyPreferences[i].Race.ToString())
                            {
                                Player04RaceComboBox.SelectedIndex = j;
                                break;
                            }
                        }
                        for (int j = 0; j < Player04ClassComboBox.Items.Count; j++)
                        {
                            if (((ComboBoxItem)Player04ClassComboBox.Items.GetItemAt(j)).Content.ToString() == _bootstrapperSettings.PartyPreferences[i].Class.ToString())
                            {
                                AssignClassOptions(i, Player04TankCheckBox, Player04HealerCheckBox);
                                Player04ClassComboBox.SelectedIndex = j;
                                break;
                            }
                        }
                        Player04TankCheckBox.IsChecked = (_bootstrapperSettings.PartyPreferences[i].Role & Role.Tank) == Role.Tank;
                        Player04DamageCheckBox.IsChecked = (_bootstrapperSettings.PartyPreferences[i].Role & Role.Damage) == Role.Damage;
                        Player04HealerCheckBox.IsChecked = (_bootstrapperSettings.PartyPreferences[i].Role & Role.Healer) == Role.Healer;
                        break;
                    case 4:
                        for (int j = 0; j < Player05RaceComboBox.Items.Count; j++)
                        {
                            if (((ComboBoxItem)Player05RaceComboBox.Items.GetItemAt(j)).Content.ToString() == _bootstrapperSettings.PartyPreferences[i].Race.ToString())
                            {
                                Player05RaceComboBox.SelectedIndex = j;
                                break;
                            }
                        }
                        for (int j = 0; j < Player05ClassComboBox.Items.Count; j++)
                        {
                            if (((ComboBoxItem)Player05ClassComboBox.Items.GetItemAt(j)).Content.ToString() == _bootstrapperSettings.PartyPreferences[i].Class.ToString())
                            {
                                AssignClassOptions(i, Player05TankCheckBox, Player05HealerCheckBox);
                                Player05ClassComboBox.SelectedIndex = j;
                                break;
                            }
                        }
                        Player05TankCheckBox.IsChecked = (_bootstrapperSettings.PartyPreferences[i].Role & Role.Tank) == Role.Tank;
                        Player05DamageCheckBox.IsChecked = (_bootstrapperSettings.PartyPreferences[i].Role & Role.Damage) == Role.Damage;
                        Player05HealerCheckBox.IsChecked = (_bootstrapperSettings.PartyPreferences[i].Role & Role.Healer) == Role.Healer;
                        break;
                }
            }

            StartAllBotsButton.IsEnabled = true;
            StopAllBotsButton.IsEnabled = false;

            InstanceCoordinator.SetActivityState(_bootstrapperSettings.PartyPreferences, _bootstrapperSettings.PartySize);
        }

        private void Start()
        {
            try
            {
                ShouldRun = true;

                _backgroundTask = Task.Run(StartSync);
            }
            catch
            {
                Console.WriteLine("Error encountered starting bot.");
            }
        }
        private void Stop()
        {
            try
            {
                ShouldRun = false;
            }
            catch
            {
                Console.WriteLine("Error encountered starting bot.");
            }
        }
        private async Task StartSync()
        {
            while (_shouldRun)
            {
                for (int i = 0; i < InstanceCoordinator.MaxActivityCapacity; i++)
                {
                    if (InstanceCoordinator._allCharacterStates[i].ProcessId > 0)
                    {
                        if (InstanceCoordinator._allCharacterStates[i].Guid == 0)
                        {
                            if (!InstanceCoordinator._allCharacterStates[i].LoginRequested)
                            {
                                var loginCommand = new InstanceCommand()
                                {
                                    StateName = InstanceCommand.LOGIN,
                                    CommandParam1 = InstanceCoordinator._partyMemberPreferences[i].Race.ToString(),
                                    CommandParam2 = InstanceCoordinator._partyMemberPreferences[i].Class.ToString(),
                                    CommandParam3 = InstanceCoordinator._partyMemberPreferences[i].Role.ToString(),
                                    CommandParam4 = Activity
                                };

                                _socketServer.SendCommandToProcess(InstanceCoordinator._allCharacterStates[i].ProcessId, loginCommand);
                                InstanceCoordinator._allCharacterStates[i].LoginRequested = true;
                            }
                        }
                        else
                        {

                        }
                    }
                }
                await Task.Delay(100);
            }
        }

        private void AssignClassOptions(int i, CheckBox tankCheckBox, CheckBox healerCheckBox)
        {
            switch (_bootstrapperSettings.PartyPreferences[i].Class)
            {
                case BloogBot.Game.Enums.Class.Druid:
                    tankCheckBox.IsEnabled = true;
                    healerCheckBox.IsEnabled = true;
                    break;
                case BloogBot.Game.Enums.Class.Hunter:
                    tankCheckBox.IsEnabled = false;
                    healerCheckBox.IsEnabled = false;
                    break;
                case BloogBot.Game.Enums.Class.Mage:
                    tankCheckBox.IsEnabled = false;
                    healerCheckBox.IsEnabled = false;
                    break;
                case BloogBot.Game.Enums.Class.Paladin:
                    tankCheckBox.IsEnabled = true;
                    healerCheckBox.IsEnabled = true;
                    break;
                case BloogBot.Game.Enums.Class.Priest:
                    tankCheckBox.IsEnabled = false;
                    healerCheckBox.IsEnabled = true;
                    break;
                case BloogBot.Game.Enums.Class.Rogue:
                    tankCheckBox.IsEnabled = false;
                    healerCheckBox.IsEnabled = false;
                    break;
                case BloogBot.Game.Enums.Class.Shaman:
                    tankCheckBox.IsEnabled = false;
                    healerCheckBox.IsEnabled = true;
                    break;
                case BloogBot.Game.Enums.Class.Warlock:
                    tankCheckBox.IsEnabled = false;
                    healerCheckBox.IsEnabled = false;
                    break;
                case BloogBot.Game.Enums.Class.Warrior:
                    tankCheckBox.IsEnabled = true;
                    healerCheckBox.IsEnabled = false;
                    break;
            }
        }

        public bool ShouldRun
        {
            get
            {
                return _shouldRun;
            }
            set
            {
                _shouldRun = value;

                StopAllBotsButton.IsEnabled = value;
                StartAllBotsButton.IsEnabled = !value;
                OnPropertyChanged(nameof(ShouldRun));
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
                OnPropertyChanged(nameof(Activity));
            }
        }
        public string Dungeon
        {
            get
            {
                return _bootstrapperSettings.Dungeon;
            }
            set
            {
                _bootstrapperSettings.Dungeon = value;
                OnPropertyChanged(nameof(Dungeon));
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
                return _bootstrapperSettings.PartySize.ToString();
            }
            set
            {
                _bootstrapperSettings.PartySize = Convert.ToInt32(value);
                OnPropertyChanged(nameof(PartySize));
            }
        }
        public string PvPSelection
        {
            get
            {
                return _bootstrapperSettings.PvPSelection;
            }
            set
            {
                _bootstrapperSettings.PvPSelection = value;
                OnPropertyChanged(nameof(PvPSelection));
            }
        }
        public PartyMemberPreference[] PartyPreferences
        {
            get
            {
                return _bootstrapperSettings.PartyPreferences;
            }
            set
            {
                _bootstrapperSettings.PartyPreferences = value;
                OnPropertyChanged(nameof(PartyPreferences));
            }
        }
        private void OnInstanceUpdate(CharacterState update)
        {
            Dispatcher.Invoke(() =>
            {
                InstanceCoordinator.ConsumeMessage(update);
                UpdateBotLabels();
            });
        }
        private void UpdateBotLabels()
        {
            for (int i = 0; i < InstanceCoordinator.MaxActivityCapacity; i++)
            {
                CharacterState characterState = InstanceCoordinator._allCharacterStates[i];

                if (characterState != null)
                {
                    switch (i)
                    {
                        case 0:
                            Player01InfoGroupBox.Header = "Info - " + characterState.ProcessId;
                            Player01StateLabel.Content = characterState.CurrentTask;
                            Player01ZoneLabel.Content = characterState.Zone;
                            Player01TaskLabel.Content = characterState.CurrentAction;
                            Player01PositionLabel.Content = characterState.Position;
                            break;
                        case 1:
                            Player02InfoGroupBox.Header = "Info - " + characterState.ProcessId;
                            Player02StateLabel.Content = characterState.CurrentTask;
                            Player02ZoneLabel.Content = characterState.Zone;
                            Player02TaskLabel.Content = characterState.CurrentAction;
                            Player02PositionLabel.Content = characterState.Position;
                            break;
                        case 2:
                            Player03InfoGroupBox.Header = "Info - " + characterState.ProcessId;
                            Player03StateLabel.Content = characterState.CurrentTask;
                            Player03ZoneLabel.Content = characterState.Zone;
                            Player03TaskLabel.Content = characterState.CurrentAction;
                            Player03PositionLabel.Content = characterState.Position;
                            break;
                        case 3:
                            Player04InfoGroupBox.Header = "Info - " + characterState.ProcessId;
                            Player04StateLabel.Content = characterState.CurrentTask;
                            Player04ZoneLabel.Content = characterState.Zone;
                            Player04TaskLabel.Content = characterState.CurrentAction;
                            Player04PositionLabel.Content = characterState.Position;
                            break;
                        case 4:
                            Player05StateLabel.Content = characterState.CurrentTask;
                            Player05ZoneLabel.Content = characterState.Zone;
                            Player05TaskLabel.Content = characterState.CurrentAction;
                            Player05InfoGroupBox.Header = "Info - " + characterState.ProcessId;
                            Player05PositionLabel.Content = characterState.Position;
                            break;
                    }
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
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }
        private void ActivityRadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                Activity = radioButton.Content.ToString();
                if (Activity == "Questing")
                {
                    PartyCheckBox.IsEnabled = true;
                }
                else
                {
                    PartyCheckBox.IsEnabled = false;
                    PartyCheckBox.IsChecked = true;

                    if (Activity == "PvP")
                    {
                        if (PvPSelection == "10 Warsong Gultch")
                        {

                        }
                        else if (PvPSelection == "15 Arathi Basin")
                        {

                        }
                        else
                        {

                        }
                    }
                    else if (Activity == "Dungeon")
                    {

                    }
                }
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
}
