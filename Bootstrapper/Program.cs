using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static Bootstrapper.WinImports;

namespace Bootstrapper
{
    class Program
    {
        class App : Application
        {
            private SocketServer _socketServer;

            public App(SocketServer socketServer)
            {
                _socketServer = socketServer;
            }

            protected override void OnStartup(StartupEventArgs e)
            {
                var mainWindow = new MainWindow(_socketServer);
                Current.MainWindow = mainWindow;
                mainWindow.Closed += (sender, args) => { Environment.Exit(0); };
                mainWindow.Show();
                

                base.OnStartup(e);
            }
        }
        
        [STAThread]
        static void Main()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var bootstrapperSettingsFilePath = Path.Combine(currentFolder, "bootstrapperSettings.json");
            var bootstrapperSettings = JsonConvert.DeserializeObject<BootstrapperSettings>(File.ReadAllText(bootstrapperSettingsFilePath));
            
            LaunchUI(LaunchServer(bootstrapperSettings), bootstrapperSettings, currentFolder);
        }

        private static void LaunchUI(SocketServer launchServer, BootstrapperSettings bootstrapperSettings,
            string currentFolder)
        {
            Application app = new App(launchServer);

            var startupInfo = new STARTUPINFO();
            LaunchProcess(bootstrapperSettings, startupInfo, currentFolder);
            app.Run();
        }

        private static SocketServer LaunchServer(BootstrapperSettings bootstrapperSettings)
        {
            var socketServer = new SocketServer(bootstrapperSettings.Port, IPAddress.Parse(bootstrapperSettings.ListenAddress));
            socketServer.Start();
            return socketServer;
        }

        private static void LaunchProcess(BootstrapperSettings bootstrapperSettings, STARTUPINFO startupInfo,
            string currentFolder)
        {
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
