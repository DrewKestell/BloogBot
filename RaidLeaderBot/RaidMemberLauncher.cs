using RaidLeaderBot.Server;
using RaidLeaderBot.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RaidLeaderBot.WinImports;

namespace RaidLeaderBot
{
    public class RaidMemberLauncher
    {
        public static RaidMemberLauncher Instance { get; private set; } = new RaidMemberLauncher();

        private RaidMemberLauncher() { }

        public async Task<int?> LaunchProcess(int raidLeaderPortNumber, ProcessTracker processTracker, CancellationToken cancellationToken)
        {
            string currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            STARTUPINFO startupInfo = new STARTUPINFO();
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

            ConfigSocketServer.Instance.AddProcessToCommandPortMapping((int)processInfo.dwProcessId, raidLeaderPortNumber);
            await Task.Delay(1000, cancellationToken);

            string loaderPath = default;
            int error = default;
            IntPtr processHandle = default, loaderPathPtr = default;

            RetryWithTimeout launchProcessRetry = new RetryWithTimeout(() =>
            {
                LaunchProcess(currentFolder, processInfo, out loaderPath, out error, out processHandle, out loaderPathPtr);
                return error <= 0;
            });
            Console.WriteLine("Here 1");
            bool launchedProcessSucceeded = await launchProcessRetry.ExecuteWithRetry(cancellationToken);

            if (!launchedProcessSucceeded)
            {
                throw new Exception($"Failed to launch process with error code {error}");
            }

            await Task.Delay(1000, cancellationToken);

            int bytesWritten = 0; // throw away
            RetryWithTimeout injectCodeRetry = new RetryWithTimeout(() =>
            {
                error = InjectCodeToProcess(loaderPath, processHandle, loaderPathPtr, ref bytesWritten);
                return error <= 0 && bytesWritten > 0;
            });
            Console.WriteLine("Here 2");
            bool injectionSucceeded = await injectCodeRetry.ExecuteWithRetry(cancellationToken);

            if (!injectionSucceeded)
            {
                CloseProcess(processHandle);
                throw new InvalidOperationException($"Failed to write Loader.dll into the WoW.exe process, error code: {error}");
            }

            IntPtr loaderDllPointer = default;
            RetryWithTimeout getLoaderDllPointerRetry = new RetryWithTimeout(() =>
            {
                GetLoaderDllPointer(out error, out loaderDllPointer);
                return error <= 0 && loaderDllPointer != IntPtr.Zero;
            });
            Console.WriteLine("Here 3");
            bool getLoaderDllPointerSucceeded = await getLoaderDllPointerRetry.ExecuteWithRetry(cancellationToken);

            if (!getLoaderDllPointerSucceeded)
            {
                CloseProcess(processHandle);
                throw new InvalidOperationException($"Failed to get memory address to Loader.dll in the WoW.exe process, error code: {error}");
            }

            RetryWithTimeout createRemoteThreadRetry = new RetryWithTimeout(() =>
            {
                return CreateRemoteThreadForLoader(processHandle, loaderDllPointer, loaderPathPtr, out error) && error <= 0;
            });
            Console.WriteLine("Here 4");
            bool createRemoteThreadSucceeded = await createRemoteThreadRetry.ExecuteWithRetry(cancellationToken);

            if (!createRemoteThreadSucceeded)
            {
                CloseProcess(processHandle);
                throw new InvalidOperationException($"Failed to create remote thread to start execution of Loader.dll in the WoW.exe process, error code: {error}");
            }

            VirtualFreeEx(processHandle, loaderPathPtr, 0, MemoryFreeType.MEM_RELEASE);

            //processTracker.AddProcess(processHandle);
            return (int)processInfo.dwProcessId;
        }

        private static bool GetLoaderDllPointer(out int error, out IntPtr loaderDllPointer)
        {
            do
            {
                loaderDllPointer = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");
                Thread.Sleep(500);
                error = Marshal.GetLastWin32Error();
            } while (error > 0);

            return error <= 0 && loaderDllPointer != IntPtr.Zero;
        }

        private static bool CreateRemoteThreadForLoader(IntPtr processHandle, IntPtr loaderDllPointer, IntPtr loaderPathPtr, out int error)
        {
            do
            {
                CreateRemoteThread(processHandle, IntPtr.Zero, IntPtr.Zero, loaderDllPointer, loaderPathPtr, 0, IntPtr.Zero);
                Thread.Sleep(500);
                error = Marshal.GetLastWin32Error();
            } while (error > 0);

            return error <= 0;
        }

        private static int InjectCodeToProcess(string loaderPath, IntPtr processHandle, IntPtr loaderPathPtr, ref int bytesWritten)
        {
            int error;
            do
            {
                // write the file path to Loader.dll to the EoE process's memory
                byte[] bytes = Encoding.Unicode.GetBytes(loaderPath);
                WriteProcessMemory(processHandle, loaderPathPtr, bytes, bytes.Length, ref bytesWritten);

                // this seems to help prevent timing issues
                Thread.Sleep(500);

                error = Marshal.GetLastWin32Error();
            } while (error > 0 || bytesWritten == 0);
            return error;
        }

        private static void LaunchProcess(string currentFolder, PROCESS_INFORMATION processInfo, out string loaderPath, out int error, out IntPtr processHandle, out IntPtr loaderPathPtr)
        {
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
        }

        private static void WaitForLoaderDllPointer(IntPtr processHandle, out int error)
        {
            do
            {
                processHandle = Process.GetProcessById(processHandle.ToInt32()).Handle;
                Thread.Sleep(500);
                error = Marshal.GetLastWin32Error();
            } while (error > 0);
        }

        private static void AllocateMemoryForLoader(string currentFolder, IntPtr processHandle, out string loaderPath, out IntPtr loaderPathPtr, out int error)
        {
            loaderPath = Path.Combine(currentFolder, "Loader.dll");
            do
            {
                loaderPathPtr = VirtualAllocEx(processHandle, IntPtr.Zero, loaderPath.Length, MemoryAllocationType.MEM_COMMIT, MemoryProtectionType.PAGE_EXECUTE_READWRITE);
                Thread.Sleep(500);
                error = Marshal.GetLastWin32Error();
            } while (error > 0);
            if (error > 0)
            {
                throw new InvalidOperationException($"Failed to allocate memory for Loader.dll, error code: {error}");
            }
        }
    }
}
