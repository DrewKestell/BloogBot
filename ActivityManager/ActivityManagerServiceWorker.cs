using System.Net;

namespace ActivityManager
{
    public class ActivityManagerServiceWorker : BackgroundService
    {
        public int Port => _listenPort;

        private readonly ILogger<ActivityManagerServiceWorker> _logger;

        private readonly IPAddress _listenAddress;
        private readonly int _listenPort;

        private readonly IPAddress _stateManagerAddress;
        private readonly int _stateManagerPort;

        private Task _backgroundTask;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"{DateTime.Now}|[WoWActivityManagerWorker]Executing update loop to WorldStateManager {_stateManagerAddress}:{_stateManagerPort}");

            while (!cancellationToken.IsCancellationRequested)
            {
                //_manager.UpdateCurrentState(cancellationToken);

                // Your worker logic here
                await Task.Delay(500, cancellationToken);
            }
        }
    }
    //public class ActivityManagerService : BackgroundService
    //{
    //    private readonly WoWActivityMemberListener _woWActivityMemberListener;
    //    private readonly StateManagerClient _stateManagerClient;

    //    private readonly Dictionary<Guid, ActivityMember> ActivityMemberStates = [];

    //    private readonly IDisposable _disposable;

    //    private Task _launchActivityMemberTask;

    //    public ActivityManagerService(ILogger<ActivityManager.ActivityManagerService> logger, IConfiguration configuration)
    //    {
    //        _listenAddress = listenAddress;
    //        _listenPort = listenPort;

    //        _woWActivityMemberListener = new(_listenAddress, _listenPort);
    //        _stateManagerClient = new(stateManagerPort, stateManagerAddress);

    //        _disposable = _woWActivityMemberListener.InstanceUpdateObservable.Subscribe(OnInstanceUpdate);

    //        _woWActivityMemberListener.Start();
    //    }

    //    Activity ActivityState { get; set; } = new Activity() { Id = Guid.NewGuid() };
    //    protected Activity CurrentActivity { get; }

    //    public void UpdateCurrentState(CancellationToken cancellationToken)
    //    {
    //        ActivityState desiredActivityState = _stateManagerClient.SendCurrentActivityState(ActivityState);

    //        if (desiredActivityState.ActivityMemberStates.Count > ActivityMemberStates.Count)
    //        {
    //            if (Process.GetProcessesByName("WoW").Length < desiredActivityState.MaxAllowedClients && (_launchActivityMemberTask == null || _launchActivityMemberTask.IsCompleted))
    //            {
    //                _launchActivityMemberTask = Task.Run(async () => await LaunchActivityMemberClient(_listenPort, cancellationToken));
    //            }
    //        }
    //        else if (desiredActivityState.ActivityMemberStates.Count < ActivityMemberStates.Count)
    //        {

    //        }

    //        for (int i = 0; i < ActivityMemberStates.Count; i++)
    //        {

    //        }
    //    }

    //    protected void OnInstanceUpdate(ActivityMemberState state)
    //    {
    //        //do stuff?
    //    }
    //    const uint PROCESS_CREATE_THREAD = 0x0002;
    //    const uint PROCESS_QUERY_INFORMATION = 0x0400;
    //    const uint PROCESS_VM_OPERATION = 0x0008;
    //    const uint PROCESS_VM_WRITE = 0x0020;
    //    const uint PROCESS_VM_READ = 0x0010;
    //    const uint MEM_COMMIT = 0x1000;
    //    const uint MEM_RESERVE = 0x2000;
    //    const uint PAGE_READWRITE = 0x04;
    //    const uint PAGE_EXECUTE_READWRITE = 0x40;
    //    public static async Task LaunchActivityMemberClient(int raidLeaderPortNumber, CancellationToken cancellationToken)
    //    {
    //        string currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    //        ProcessStartInfo wowStartInfo = new("E:\\repos\\Elysium Project Game Client\\WoW.exe")
    //        {
    //            //UseShellExecute = false,
    //            //CreateNoWindow = true
    //        };

    //        Console.WriteLine("Process.Start(wowStartInfo)");
    //        Process wowClientProcess = Process.Start(wowStartInfo);
    //        if (wowClientProcess == null)
    //        {
    //            Console.WriteLine("Failed to start WoW client.");
    //        }

    //        await Task.Delay(5000);

    //        //// Open the WoW client process
    //        nint hProcess = OpenProcess((int)(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ), false, wowClientProcess.Id);

    //        if (hProcess == nint.Zero)
    //        {
    //            Console.WriteLine("Failed to open WoW client process. Error code: " + Marshal.GetLastWin32Error());
    //            throw new InvalidOperationException($"Failed to open WoW client process");
    //        }

    //        string loaderPathString = "E:\\repos\\BloogBot\\Bot\\Debug\\Loader.dll";
    //        byte[] loaderPathBytes = Encoding.UTF8.GetBytes(loaderPathString.ToCharArray());

    //        if (!File.Exists(loaderPathString))
    //        {
    //            Console.WriteLine($"Failed to find DLL at {loaderPathString}");
    //            throw new Exception($"Failed to find DLL at {loaderPathString}");
    //        }

    //        // allocate enough memory to hold the full file path to Loader.dll within the BloogBot process
    //        nint injectedDLLPathPtr = VirtualAllocEx(
    //                hProcess,
    //                0,
    //                loaderPathBytes.Length,
    //                MemoryAllocationType.MEM_COMMIT,
    //                MemoryProtectionType.PAGE_EXECUTE_READWRITE);

    //        int bytesWritten = 0; // throw away

    //        // write the file path to Loader.dll to the WoW process's memory
    //        WriteProcessMemory(hProcess, injectedDLLPathPtr, loaderPathBytes, loaderPathBytes.Length, ref bytesWritten);

    //        Console.WriteLine($"loaderPathBytes.Length {loaderPathBytes.Length} | bytesWritten {bytesWritten} | loaderPathString.Length {loaderPathString.Length}");
    //        if (bytesWritten != loaderPathBytes.Length)
    //        {
    //            CloseProcess(hProcess);
    //            throw new InvalidOperationException($"Failed to write Loader.dll into the WoW.exe process, error code: {bytesWritten} {loaderPathBytes.Length}");
    //        }

    //        // search current process's for the memory address of the LoadLibraryW function within the kernel32.dll module
    //        nint loaderDllPointer = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

    //        await Task.Delay(1000);
    //        CreateRemoteThread(hProcess, nint.Zero, nint.Zero, loaderDllPointer, injectedDLLPathPtr, 0, nint.Zero);

    //        await Task.Delay(1000);
    //        VirtualFreeEx(hProcess, injectedDLLPathPtr, 0, MemoryFreeType.MEM_RELEASE);
    //        Console.WriteLine("VirtualFreeEx");
    //    }

    //    private static bool GetLoaderDllPointer(out int error, out nint loaderDllPointer)
    //    {
    //        do
    //        {
    //            loaderDllPointer = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");

    //            Thread.Sleep(500);
    //            error = Marshal.GetLastWin32Error();

    //        } while (error > 0);

    //        return error <= 0 && loaderDllPointer != nint.Zero;
    //    }

    //    private static bool CreateRemoteThreadForLoader(nint processHandle, nint loaderDllPointer, nint loaderPathPtr, out int error)
    //    {
    //        do
    //        {
    //            CreateRemoteThread(processHandle, nint.Zero, nint.Zero, loaderDllPointer, loaderPathPtr, 0, nint.Zero);
    //            Thread.Sleep(500);
    //            error = Marshal.GetLastWin32Error();
    //        } while (error > 0);

    //        return error <= 0;
    //    }

    //    private static int WriteDLLPathToMemory(byte[] bytes, nint processHandle, nint loaderPathPtr, ref int bytesWritten)
    //    {
    //        int error;
    //        do
    //        {
    //            // write the file path to Loader.dll to the EoE process's memory
    //            WriteProcessMemory(processHandle, loaderPathPtr, bytes, bytes.Length, ref bytesWritten);

    //            // this seems to help prevent timing issues
    //            Thread.Sleep(500);

    //            error = Marshal.GetLastWin32Error();

    //        } while (error > 0 || bytesWritten == 0);
    //        return error;
    //    }

    //    private static nint AllocateMemoryForInjectedDLLPath(nint processHandle, byte[] loaderPathBytes, out int error)
    //    {
    //        nint injectedDLLPathPtr;
    //        do
    //        {
    //            // allocate enough memory to hold the full file path to Loader.dll within the WoW.exe process
    //            injectedDLLPathPtr = VirtualAllocEx(
    //                processHandle,
    //                0,
    //                loaderPathBytes.Length,
    //                MemoryAllocationType.MEM_COMMIT,
    //                MemoryProtectionType.PAGE_EXECUTE_READWRITE);

    //            // this seems to help prevent timing issues
    //            Thread.Sleep(500);

    //            error = Marshal.GetLastWin32Error();

    //        } while (error > 0);

    //        if (error > 0)
    //            throw new InvalidOperationException($"Failed to allocate memory for Loader.dll, error code: {error}");

    //        return injectedDLLPathPtr;
    //    }
    //}
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