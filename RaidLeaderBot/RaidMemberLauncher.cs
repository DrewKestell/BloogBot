using RaidLeaderBot.Server;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static RaidLeaderBot.WinImports;

namespace RaidLeaderBot
{
    public class RaidMemberLauncher
    {
        public static RaidMemberLauncher Instance { get; private set; } = new RaidMemberLauncher();
        private RaidMemberLauncher() { }
        public int LaunchProcess(int raidLeaderPortNumber)
        {
            string currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            STARTUPINFO startupInfo = new STARTUPINFO();
            // run WoW.exe in a new process
            CreateProcess(
                RaidLeaderBotSettings.Instance.PathToWoW,
                null,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                ProcessCreationFlag.CREATE_DEFAULT_ERROR_MODE,
                IntPtr.Zero,
                null,
                ref startupInfo,
                out PROCESS_INFORMATION processInfo);

            ConfigSockerServer.Instance.AddProcessToCommandPortMapping((int)processInfo.dwProcessId, raidLeaderPortNumber);

            // this seems to help prevent timing issues
            Thread.Sleep(1000);
            string loaderPath;
            int error;
            IntPtr processHandle;
            IntPtr loaderPathPtr;
            do
            {
                // get a handle to the WoW process
                processHandle = Process.GetProcessById((int)processInfo.dwProcessId).Handle;

                // resolve the file path to Loader.dll relative to our current working directory
                loaderPath = Path.Combine(currentFolder, "Loader.dll");

                // allocate enough memory to hold the full file path to Loader.dll within the WoW.exe process
                loaderPathPtr = VirtualAllocEx(
                    processHandle,
                    (IntPtr)0,
                    loaderPath.Length,
                    MemoryAllocationType.MEM_COMMIT,
                    MemoryProtectionType.PAGE_EXECUTE_READWRITE);

                // this seems to help prevent timing issues
                Thread.Sleep(500);

                error = Marshal.GetLastWin32Error();
            } while (error > 0);

            if (error > 0)
                throw new InvalidOperationException($"Failed to allocate memory for Loader.dll, error code: {error}");
            
            int bytesWritten = 0; // throw away
            do
            {
                // write the file path to Loader.dll to the EoE process's memory
                byte[] bytes = Encoding.Unicode.GetBytes(loaderPath);                
                WriteProcessMemory(processHandle, loaderPathPtr, bytes, bytes.Length, ref bytesWritten);

                // this seems to help prevent timing issues
                Thread.Sleep(500);

                error = Marshal.GetLastWin32Error();
            } while (error > 0 || bytesWritten == 0);

            if (error > 0 || bytesWritten == 0)
                throw new InvalidOperationException(
                    $"Failed to write Loader.dll into the WoW.exe process, error code: {error}");
            IntPtr loaderDllPointer;
            do
            {
                // search current process's for the memory address of the LoadLibraryW function within the kernel32.dll module
                loaderDllPointer = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");

                // this seems to help prevent timing issues
                Thread.Sleep(500);


                error = Marshal.GetLastWin32Error();
            } while (error > 0);

            if (error > 0)
                throw new InvalidOperationException(
                    $"Failed to get memory address to Loader.dll in the WoW.exe process, error code: {error}");

            do
            {
                // create a new thread with the execution starting at the LoadLibraryW function, 
                // with the path to our Loader.dll passed as a parameter
                CreateRemoteThread(processHandle, (IntPtr)null, (IntPtr)0, loaderDllPointer, loaderPathPtr, 0, (IntPtr)null);

                // this seems to help prevent timing issues
                Thread.Sleep(500);

                error = Marshal.GetLastWin32Error();
            } while (error > 0);

            if (error > 0)
                throw new InvalidOperationException(
                    $"Failed to create remote thread to start execution of Loader.dll in the WoW.exe process, error code: {error}");

            // free the memory that was allocated by VirtualAllocEx
            VirtualFreeEx(processHandle, loaderPathPtr, 0, MemoryFreeType.MEM_RELEASE);

            return (int)processInfo.dwProcessId;
        }
    }
}
