
using BloogBot.Models.Dto;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

        private static readonly string[] Races = { "Human", "Dwarf", "Night Elf", "Gnome", "Orc", "Undead", "Tauren", "Troll" };
        private static readonly string[] Classes = { "Druid", "Hunter", "Mage", "Paladin", "Priest", "Rogue", "Shaman", "Warlock", "Warrior" };

        private static readonly string[] HumanClasses = { "Mage", "Paladin", "Priest", "Rogue", "Warlock", "Warrior" };
        private static readonly string[] DwarfClasses = { "Hunter", "Paladin", "Priest", "Rogue", "Warrior" };
        private static readonly string[] NightElfClasses = { "Druid", "Hunter", "Priest", "Rogue", "Warrior" };
        private static readonly string[] GnomeClasses = { "Mage", "Rogue", "Warlock", "Warrior" };

        private static readonly string[] OrcClasses = { "Hunter", "Rogue", "Shaman", "Warlock", "Warrior" };
        private static readonly string[] UndeadClasses = { "Mage", "Priest", "Rogue", "Warlock", "Warrior" };
        private static readonly string[] TaurenClasses = { "Druid", "Hunter", "Shaman", "Warrior" };
        private static readonly string[] TrollClasses = { "Hunter", "Mage", "Priest", "Rogue", "Shaman", "Warrior" };

        public MainWindow(SocketServer socketServer, BootstrapperSettings bootstrapperSettings)
        {
            InitializeComponent();
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

            } else
            {

            }

            PartyCheckBox.IsChecked = _bootstrapperSettings.ShouldParty;
        }

        private void OnInstanceUpdate(InstanceUpdate update)
        {
            Dispatcher.Invoke(() =>
            {
                if (update != null)
                {
                    InstanceCoordinator.ConsumeMessage(update);
                }

                UpdateBotLabels();
            });
        }

        private void UpdateBotLabels()
        {
            for (int i = 0; i < 5; i++)
            {
                InstanceUpdate currentUpdate = InstanceCoordinator._allInstances[i];

                if (currentUpdate != null)
                {
                    switch (i)
                    {
                        case 0:
                            Player01Grid.Visibility = Visibility.Visible;
                            Player01InfoGroupBox.Header = "Info - " + currentUpdate.ProcessId;
                            Player01StateLabel.Content = currentUpdate.CurrentTask;
                            Player01ZoneLabel.Content = currentUpdate.Zone;
                            Player01TaskLabel.Content = currentUpdate.CurrentAction;
                            Player01PositionLabel.Content = currentUpdate.Position;
                            break;
                        case 1:
                            Player02Grid.Visibility = Visibility.Visible;
                            Player02InfoGroupBox.Header = "Info - " + currentUpdate.ProcessId;
                            Player02StateLabel.Content = currentUpdate.CurrentTask;
                            Player02ZoneLabel.Content = currentUpdate.Zone;
                            Player02TaskLabel.Content = currentUpdate.CurrentAction;
                            Player02PositionLabel.Content = currentUpdate.Position;
                            break;
                        case 2:
                            Player03Grid.Visibility = Visibility.Visible;
                            Player03InfoGroupBox.Header = "Info - " + currentUpdate.ProcessId;
                            Player03StateLabel.Content = currentUpdate.CurrentTask;
                            Player03ZoneLabel.Content = currentUpdate.Zone;
                            Player03TaskLabel.Content = currentUpdate.CurrentAction;
                            Player03PositionLabel.Content = currentUpdate.Position;
                            break;
                        case 3:
                            Player04Grid.Visibility = Visibility.Visible;
                            Player04InfoGroupBox.Header = "Info - " + currentUpdate.ProcessId;
                            Player04StateLabel.Content = currentUpdate.CurrentTask;
                            Player04ZoneLabel.Content = currentUpdate.Zone;
                            Player04TaskLabel.Content = currentUpdate.CurrentAction;
                            Player04PositionLabel.Content = currentUpdate.Position;
                            break;
                        case 4:
                            Player05Grid.Visibility = Visibility.Visible;
                            Player05StateLabel.Content = currentUpdate.CurrentTask;
                            Player05ZoneLabel.Content = currentUpdate.Zone;
                            Player05TaskLabel.Content = currentUpdate.CurrentAction;
                            Player05InfoGroupBox.Header = "Info - " + currentUpdate.ProcessId;
                            Player05PositionLabel.Content = currentUpdate.Position;
                            break;
                    }
                }
                else
                {
                    switch (i)
                    {
                        case 0:
                            Player01Grid.Visibility = Visibility.Collapsed;
                            break;
                        case 1:
                            Player02Grid.Visibility = Visibility.Collapsed;
                            break;
                        case 2:
                            Player03Grid.Visibility = Visibility.Collapsed;
                            break;
                        case 3:
                            Player04Grid.Visibility = Visibility.Collapsed;
                            break;
                        case 4:
                            Player05Grid.Visibility = Visibility.Collapsed;
                            break;
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
