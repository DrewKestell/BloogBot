using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using WoWActivityManager.Clients;
using WoWActivityManager.Listeners;
using WoWActivityMember.Models;
using static WinProcessImports.WinImports;

namespace WoWActivityManager
{
    public class WoWActivityManager
    {
        private readonly IPAddress _listenAddress;
        private readonly int _listenPort;
        private readonly WoWActivityMemberListener _woWActivityMemberListener;
        private readonly WoWStateManagerClient _stateManagerClient;
        private readonly Dictionary<Guid, ActivityMemberState> ActivityMemberStates = [];
        private readonly IDisposable _disposable;

        public WoWActivityManager(IPAddress listenAddress, int listenPort, IPAddress stateManagerAddress, int stateManagerPort)
        {
            _listenAddress = listenAddress;
            _listenPort = listenPort;

            _woWActivityMemberListener = new(_listenAddress, _listenPort);
            _stateManagerClient = new(stateManagerPort, stateManagerAddress);

            _disposable = _woWActivityMemberListener.InstanceUpdateObservable.Subscribe(OnInstanceUpdate);

            _woWActivityMemberListener.Start();
        }

        ActivityState ActivityState { get; set; } = new ActivityState() { ServiceId = Guid.NewGuid() };
        protected ActivityState CurrentActivity { get; }

        public async Task UpdateCurrentState(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ActivityState desiredActivityState = _stateManagerClient.SendCurrentActivityState(ActivityState);

                if (desiredActivityState.ActivityMemberStates.Count > ActivityMemberStates.Count)
                {
                    try
                    {
                        if (Process.GetProcessesByName("WoW").Length < desiredActivityState.MaxAllowedClients)
                        {
                            await LaunchActivityMemberClient(_listenPort, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                else if (desiredActivityState.ActivityMemberStates.Count < ActivityMemberStates.Count)
                {

                }

                for (int i = 0; i < ActivityMemberStates.Count; i++)
                {

                }

                await Task.Delay(1000, cancellationToken);
            }

            _disposable.Dispose();
        }

        protected void OnInstanceUpdate(ActivityMemberState state)
        {
            //do stuff?
        }
        const int PROCESS_CREATE_THREAD = 0x0002;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_READ = 0x0010;

        const uint MEM_COMMIT = 0x1000;
        const uint MEM_RESERVE = 0x2000;
        const uint PAGE_READWRITE = 0x04;
        public static async Task<int?> LaunchActivityMemberClient(int raidLeaderPortNumber, CancellationToken cancellationToken)
        {
            // Start the first process
            Process process = new();
            process.StartInfo.FileName = "E:\\repos\\Elysium Project Game Client\\WoW.exe";
            process.Start();

            // Open the process
            IntPtr hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION |
                                          PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                                          false, process.Id);

            if (hProcess == IntPtr.Zero)
            {
                Console.WriteLine("Failed to open process");
                return 1;
            }
            await Task.Delay(500, cancellationToken);

            // Path to the DLL to inject
            string dllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Loader.dll");

            byte[] dllPathBytes = Encoding.ASCII.GetBytes(dllPath);

            // Allocate memory for the DLL path
            IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)dllPathBytes.Length, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (allocMemAddress == IntPtr.Zero)
            {
                Console.WriteLine("Failed to allocate memory");
                return 2;
            }
            await Task.Delay(500, cancellationToken);

            // Write the DLL path to the allocated memory
            IntPtr bytesWritten;
            if (!WriteProcessMemory(hProcess, allocMemAddress, dllPathBytes, (uint)dllPathBytes.Length, out bytesWritten))
            {
                Console.WriteLine("Failed to write process memory");
                return 3;
            }
            await Task.Delay(500, cancellationToken);

            // Get the address of LoadLibraryA
            IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

            // Create the remote thread to load the DLL
            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, out _);
            if (hThread == IntPtr.Zero)
            {
                Console.WriteLine("Failed to create remote thread");
                return 4;
            }

            Console.WriteLine($"{dllPathBytes.Length}");

            //    string currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //    STARTUPINFO startupInfo = new();
            //    CreateProcess(
            //        "E:\\repos\\Elysium Project Game Client\\WoW.exe",
            //        null,
            //        IntPtr.Zero,
            //        IntPtr.Zero,
            //        false,
            //        ProcessCreationFlag.CREATE_DEFAULT_ERROR_MODE,
            //        IntPtr.Zero,
            //        null,
            //        ref startupInfo,
            //        out PROCESS_INFORMATION processInfo);

            //    await Task.Delay(500, cancellationToken);

            //    string loaderPath = default;
            //    int error = default;
            //    IntPtr processHandle = default, loaderPathPtr = default;

            //    RetryWithTimeout launchProcessRetry = new(() =>
            //    {
            //        AllocateMemoryForDLLPath(currentFolder, processInfo, out loaderPath, out error, out processHandle, out loaderPathPtr);
            //        return error <= 0;
            //    });

            //    bool launchedProcessSucceeded = await launchProcessRetry.ExecuteWithRetry(cancellationToken);

            //    if (!launchedProcessSucceeded)
            //    {
            //        throw new Exception($"Failed to launch process with error code {error}");
            //    }

            //    await Task.Delay(500, cancellationToken);

            //    int bytesWritten = 0; // throw away
            //    RetryWithTimeout injectCodeRetry = new(() =>
            //    {
            //        error = InjectCodeToProcess(loaderPath, processHandle, loaderPathPtr, ref bytesWritten);
            //        return error <= 0 && bytesWritten > 0;
            //    });

            //    bool injectionSucceeded = await injectCodeRetry.ExecuteWithRetry(cancellationToken);

            //    if (!injectionSucceeded)
            //    {
            //        CloseProcess(processHandle);
            //        throw new InvalidOperationException($"Failed to write Loader.dll into the WoW.exe process, error code: {error}");
            //    }

            //    await Task.Delay(1000, cancellationToken);

            //    IntPtr loaderDllPointer = default;
            //    RetryWithTimeout getLoaderDllPointerRetry = new(() =>
            //    {
            //        GetLoaderDllPointer(out error, out loaderDllPointer);
            //        return error <= 0 && loaderDllPointer != IntPtr.Zero;
            //    });

            //    bool getLoaderDllPointerSucceeded = await getLoaderDllPointerRetry.ExecuteWithRetry(cancellationToken);

            //    if (!getLoaderDllPointerSucceeded)
            //    {
            //        CloseProcess(processHandle);
            //        throw new InvalidOperationException($"Failed to get memory address to Loader.dll in the WoW.exe process, error code: {error}");
            //    }

            //    await Task.Delay(1000, cancellationToken);

            //    RetryWithTimeout createRemoteThreadRetry = new(() =>
            //    {
            //        return CreateRemoteThreadForLoader(processHandle, loaderDllPointer, loaderPathPtr, out error) && error <= 0;
            //    });

            //    bool createRemoteThreadSucceeded = await createRemoteThreadRetry.ExecuteWithRetry(cancellationToken);

            //    if (!createRemoteThreadSucceeded)
            //    {
            //        CloseProcess(processHandle);
            //        throw new InvalidOperationException($"Failed to create remote thread to start execution of Loader.dll in the WoW.exe process, error code: {error}");
            //    }
            //    await Task.Delay(5000, cancellationToken);
            //    VirtualFreeEx(processHandle, loaderPathPtr, 0, MemoryFreeType.MEM_RELEASE);

            return 0;
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

        private static void AllocateMemoryForDLLPath(string currentFolder, PROCESS_INFORMATION processInfo, out string loaderPath, out int error, out IntPtr processHandle, out IntPtr loaderPathPtr)
        {
            do
            {
                // resolve the file path to Loader.dll relative to our current working directory
                loaderPath = Path.Combine(currentFolder, "Loader.dll");

                // get a handle to the WoW process
                processHandle = Process.GetProcessById((int)processInfo.dwProcessId).Handle;
                // allocate enough memory to hold the full file path to Loader.dll within the WoW.exe process
                loaderPathPtr = VirtualAllocEx(
                    processHandle,
                    0,
                    loaderPath.Length,
                    MemoryAllocationType.MEM_COMMIT,
                    MemoryProtectionType.PAGE_EXECUTE_READWRITE);

                // this seems to help prevent timing issues
                Thread.Sleep(1000);

                error = Marshal.GetLastWin32Error();
            } while (error > 0);

            if (error > 0)
                throw new InvalidOperationException($"Failed to allocate memory for Loader.dll, error code: {error}");
        }
    }
    public class RetryWithTimeout(Func<bool> function, TimeSpan? timeout = null, TimeSpan? stepTimeSpan = null)
    {
        private readonly static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);
        private readonly static TimeSpan DefaultStepTimeSpan = TimeSpan.FromSeconds(1);

        private readonly Func<bool> _function = function ?? throw new ArgumentNullException(nameof(function));
        private readonly TimeSpan _timeout = timeout ?? DefaultTimeout;
        private readonly TimeSpan _stepTimeSpan = stepTimeSpan ?? DefaultStepTimeSpan;

        public async Task<bool> ExecuteWithRetry(CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;
            while (DateTime.Now - startTime < _timeout)
            {
                if (_function())
                {
                    return true;
                }
                await Task.Delay(_stepTimeSpan, cancellationToken);
            }
            return false; // Timeout reached
        }
    }

    public class RetryWithTimeoutAsync(Func<Task<bool>> function, TimeSpan? timeout = null, TimeSpan? stepTimeSpan = null)
    {
        private readonly static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);
        private readonly static TimeSpan DefaultStepTimeSpan = TimeSpan.FromSeconds(1);

        private readonly Func<Task<bool>> _function = function ?? throw new ArgumentNullException(nameof(function));
        private readonly TimeSpan _timeout = timeout ?? DefaultTimeout;
        private readonly TimeSpan _stepTimeSpan = stepTimeSpan ?? DefaultStepTimeSpan;

        public async Task<bool> ExecuteWithRetry(CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;
            while (DateTime.Now - startTime < _timeout)
            {
                if (await _function())
                {
                    return true;
                }
                await Task.Delay(_stepTimeSpan, cancellationToken);
            }
            return false; // Timeout reached
        }
    }
}
