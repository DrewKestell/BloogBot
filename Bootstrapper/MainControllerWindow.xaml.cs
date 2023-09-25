
using BloogBot.Models.Dto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
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
    public partial class MainWindow : Window
    {
        private readonly SocketServer _socketServer;
        private readonly BootstrapperSettings _bootstrapperSettings;

        private static readonly string[] Races =
            { "Human", "Dwarf", "Night Elf", "Gnome", "Orc", "Undead", "Tauren", "Troll" };

        private static readonly string[] Classes =
            { "Druid", "Hunter", "Mage", "Paladin", "Priest", "Rogue", "Shaman", "Warlock", "Warrior" };

        private static readonly string[] HumanClasses = { "Mage", "Paladin", "Priest", "Rogue", "Warlock", "Warrior" };
        private static readonly string[] DwarfClasses = { "Hunter", "Paladin", "Priest", "Rogue", "Warrior" };
        private static readonly string[] NightElfClasses = { "Druid", "Hunter", "Priest", "Rogue", "Warrior" };
        private static readonly string[] GnomeClasses = { "Mage", "Rogue", "Warlock", "Warrior" };

        private static readonly string[] OrcClasses = { "Hunter", "Rogue", "Shaman", "Warlock", "Warrior" };
        private static readonly string[] UndeadClasses = { "Mage", "Priest", "Rogue", "Warlock", "Warrior" };
        private static readonly string[] TaurenClasses = { "Druid", "Hunter", "Shaman", "Warrior" };
        private static readonly string[] TrollClasses = { "Hunter", "Mage", "Priest", "Rogue", "Shaman", "Warrior" };

        public sealed class PlayerViewModel : INotifyPropertyChanged
        {
            private Visibility _visibility;

            public Visibility Visibility
            {
                get => _visibility;
                set
                {
                    _visibility = value;
                    OnPropertyChanged();
                }
            }

            private string _header;

            public string Header
            {
                get => _header;
                set
                {
                    _header = value;
                    OnPropertyChanged();
                }
            }

            private string _currentTask;

            public string CurrentTask
            {
                get => _currentTask;
                set
                {
                    _currentTask = value;
                    OnPropertyChanged();
                }
            }

            private string _currentAction;

            public string CurrentAction
            {
                get => _currentAction;
                set
                {
                    _currentAction = value;
                    OnPropertyChanged();
                }
            }

            private string _position;

            public string Position
            {
                get => _position;
                set
                {
                    _position = value;
                    OnPropertyChanged();
                }
            }

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

        public ObservableCollection<PlayerViewModel> Players { get; set; } = new ObservableCollection<PlayerViewModel>();
        
        private Task _backgroundTask;


        public MainWindow(SocketServer socketServer, BootstrapperSettings bootstrapperSettings)
        {
            InitializeComponent();
            DataContext = this;
            _socketServer = socketServer;
            _socketServer.InstanceUpdateObservable.Subscribe(OnInstanceUpdate);
            _bootstrapperSettings = bootstrapperSettings;

            if (_bootstrapperSettings.Activity == "Dungeon")
            {

            }
            else if (_bootstrapperSettings.Activity == "Questing")
            {

            }
            else if (_bootstrapperSettings.Activity == "PvP")
            {

            }

            if (_bootstrapperSettings.DungeonPartySize == 5)
            {

            }
            else
            {

            }
            
            var random = new Random();
            _backgroundTask = Task.Run(async () =>
            {
                //var possibleNames = {{}}
                while (true)
                {
                    if (Players.Count <= 40 && (Players.Count <= 3 || random.Next(1) == 0))
                    {
                        var player = new PlayerViewModel
                        {
                            Visibility = Visibility.Visible,
                            Header = "Player " + (Players.Count + 1),
                            CurrentTask = "Questing",
                            CurrentAction = "Killing mobs",
                            Position = $"X: {random.Next(100,500)}, Y: {random.Next(100,500)}"
                        };
                        await Dispatcher.Invoke(async () =>
                        {
                            Players.Add(player);
                        });
                    }
                    else
                    {
                        await Dispatcher.Invoke(async () =>
                        {
                            Players.RemoveAt(random.Next(Players.Count));
                        });
                    }

                    await Task.Delay(random.Next(1000, 5000));
                }
            });
            
            PartyCheckBox.IsChecked = _bootstrapperSettings.ShouldParty;
        }

        private void OnInstanceUpdate(InstanceUpdate update)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    if (update != null)
            //    {
            //        InstanceCoordinator.ConsumeMessage(update);
            //    }

            //    UpdateBotLabels();
            //});
        }

        private void UpdateBotLabels()
        {
            
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

        private void NewWoWLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            LaunchProcess();
        }

        private void PlayerStartButton_Click(object sender, RoutedEventArgs e)
        {
            Button playerStart = sender as Button;
            if (playerStart != null)
            {

            }
        }
    }
}
