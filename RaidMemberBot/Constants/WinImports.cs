using RaidMemberBot.ExtensionMethods;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Shapes;

namespace RaidMemberBot.Constants
{
    internal static class WinImports
    {
        [DllImport("ntdll.dll")]
        internal static extern int NtQueryVirtualMemoryDelegate(
            IntPtr processHandle, IntPtr baseAddress, MemoryInformationClass memoryInformationClass,
            IntPtr memoryInformation, ulong length, ref ulong returnLength);

        internal enum MemoryInformationClass : uint
        {
            MemoryBasicInformation = 0,
            MemoryWorkingSetList = 1,
            MemorySectionName = 2,
            MemoryBasicVlmInformation = 3
        }

        internal enum NtStatus : uint
        {
            // Success
            Success = 0x00000000,
            Wait0 = 0x00000000,
            Wait1 = 0x00000001,
            Wait2 = 0x00000002,
            Wait3 = 0x00000003,
            Wait63 = 0x0000003f,
            Abandoned = 0x00000080,
            AbandonedWait0 = 0x00000080,
            AbandonedWait1 = 0x00000081,
            AbandonedWait2 = 0x00000082,
            AbandonedWait3 = 0x00000083,
            AbandonedWait63 = 0x000000bf,
            UserApc = 0x000000c0,
            KernelApc = 0x00000100,
            Alerted = 0x00000101,
            Timeout = 0x00000102,
            Pending = 0x00000103,
            Reparse = 0x00000104,
            MoreEntries = 0x00000105,
            NotAllAssigned = 0x00000106,
            SomeNotMapped = 0x00000107,
            OpLockBreakInProgress = 0x00000108,
            VolumeMounted = 0x00000109,
            RxActCommitted = 0x0000010a,
            NotifyCleanup = 0x0000010b,
            NotifyEnumDir = 0x0000010c,
            NoQuotasForAccount = 0x0000010d,
            PrimaryTransportConnectFailed = 0x0000010e,
            PageFaultTransition = 0x00000110,
            PageFaultDemandZero = 0x00000111,
            PageFaultCopyOnWrite = 0x00000112,
            PageFaultGuardPage = 0x00000113,
            PageFaultPagingFile = 0x00000114,
            CrashDump = 0x00000116,
            ReparseObject = 0x00000118,
            NothingToTerminate = 0x00000122,
            ProcessNotInJob = 0x00000123,
            ProcessInJob = 0x00000124,
            ProcessCloned = 0x00000129,
            FileLockedWithOnlyReaders = 0x0000012a,
            FileLockedWithWriters = 0x0000012b,

            // Informational
            Informational = 0x40000000,
            ObjectNameExists = 0x40000000,
            ThreadWasSuspended = 0x40000001,
            WorkingSetLimitRange = 0x40000002,
            ImageNotAtBase = 0x40000003,
            RegistryRecovered = 0x40000009,

            // Warning
            Warning = 0x80000000,
            GuardPageViolation = 0x80000001,
            DatatypeMisalignment = 0x80000002,
            Breakpoint = 0x80000003,
            SingleStep = 0x80000004,
            BufferOverflow = 0x80000005,
            NoMoreFiles = 0x80000006,
            HandlesClosed = 0x8000000a,
            PartialCopy = 0x8000000d,
            DeviceBusy = 0x80000011,
            InvalidEaName = 0x80000013,
            EaListInconsistent = 0x80000014,
            NoMoreEntries = 0x8000001a,
            LongJump = 0x80000026,
            DllMightBeInsecure = 0x8000002b,

            // Error
            Error = 0xc0000000,
            Unsuccessful = 0xc0000001,
            NotImplemented = 0xc0000002,
            InvalidInfoClass = 0xc0000003,
            InfoLengthMismatch = 0xc0000004,
            AccessViolation = 0xc0000005,
            InPageError = 0xc0000006,
            PagefileQuota = 0xc0000007,
            InvalidHandle = 0xc0000008,
            BadInitialStack = 0xc0000009,
            BadInitialPc = 0xc000000a,
            InvalidCid = 0xc000000b,
            TimerNotCanceled = 0xc000000c,
            InvalidParameter = 0xc000000d,
            NoSuchDevice = 0xc000000e,
            NoSuchFile = 0xc000000f,
            InvalidDeviceRequest = 0xc0000010,
            EndOfFile = 0xc0000011,
            WrongVolume = 0xc0000012,
            NoMediaInDevice = 0xc0000013,
            NoMemory = 0xc0000017,
            NotMappedView = 0xc0000019,
            UnableToFreeVm = 0xc000001a,
            UnableToDeleteSection = 0xc000001b,
            IllegalInstruction = 0xc000001d,
            AlreadyCommitted = 0xc0000021,
            AccessDenied = 0xc0000022,
            BufferTooSmall = 0xc0000023,
            ObjectTypeMismatch = 0xc0000024,
            NonContinuableException = 0xc0000025,
            BadStack = 0xc0000028,
            NotLocked = 0xc000002a,
            NotCommitted = 0xc000002d,
            InvalidParameterMix = 0xc0000030,
            ObjectNameInvalid = 0xc0000033,
            ObjectNameNotFound = 0xc0000034,
            ObjectNameCollision = 0xc0000035,
            ObjectPathInvalid = 0xc0000039,
            ObjectPathNotFound = 0xc000003a,
            ObjectPathSyntaxBad = 0xc000003b,
            DataOverrun = 0xc000003c,
            DataLate = 0xc000003d,
            DataError = 0xc000003e,
            CrcError = 0xc000003f,
            SectionTooBig = 0xc0000040,
            PortConnectionRefused = 0xc0000041,
            InvalidPortHandle = 0xc0000042,
            SharingViolation = 0xc0000043,
            QuotaExceeded = 0xc0000044,
            InvalidPageProtection = 0xc0000045,
            MutantNotOwned = 0xc0000046,
            SemaphoreLimitExceeded = 0xc0000047,
            PortAlreadySet = 0xc0000048,
            SectionNotImage = 0xc0000049,
            SuspendCountExceeded = 0xc000004a,
            ThreadIsTerminating = 0xc000004b,
            BadWorkingSetLimit = 0xc000004c,
            IncompatibleFileMap = 0xc000004d,
            SectionProtection = 0xc000004e,
            EasNotSupported = 0xc000004f,
            EaTooLarge = 0xc0000050,
            NonExistentEaEntry = 0xc0000051,
            NoEasOnFile = 0xc0000052,
            EaCorruptError = 0xc0000053,
            FileLockConflict = 0xc0000054,
            LockNotGranted = 0xc0000055,
            DeletePending = 0xc0000056,
            CtlFileNotSupported = 0xc0000057,
            UnknownRevision = 0xc0000058,
            RevisionMismatch = 0xc0000059,
            InvalidOwner = 0xc000005a,
            InvalidPrimaryGroup = 0xc000005b,
            NoImpersonationToken = 0xc000005c,
            CantDisableMandatory = 0xc000005d,
            NoLogonServers = 0xc000005e,
            NoSuchLogonSession = 0xc000005f,
            NoSuchPrivilege = 0xc0000060,
            PrivilegeNotHeld = 0xc0000061,
            InvalidAccountName = 0xc0000062,
            UserExists = 0xc0000063,
            NoSuchUser = 0xc0000064,
            GroupExists = 0xc0000065,
            NoSuchGroup = 0xc0000066,
            MemberInGroup = 0xc0000067,
            MemberNotInGroup = 0xc0000068,
            LastAdmin = 0xc0000069,
            WrongPassword = 0xc000006a,
            IllFormedPassword = 0xc000006b,
            PasswordRestriction = 0xc000006c,
            LogonFailure = 0xc000006d,
            AccountRestriction = 0xc000006e,
            InvalidLogonHours = 0xc000006f,
            InvalidWorkstation = 0xc0000070,
            PasswordExpired = 0xc0000071,
            AccountDisabled = 0xc0000072,
            NoneMapped = 0xc0000073,
            TooManyLuidsRequested = 0xc0000074,
            LuidsExhausted = 0xc0000075,
            InvalidSubAuthority = 0xc0000076,
            InvalidAcl = 0xc0000077,
            InvalidSid = 0xc0000078,
            InvalidSecurityDescr = 0xc0000079,
            ProcedureNotFound = 0xc000007a,
            InvalidImageFormat = 0xc000007b,
            NoToken = 0xc000007c,
            BadInheritanceAcl = 0xc000007d,
            RangeNotLocked = 0xc000007e,
            DiskFull = 0xc000007f,
            ServerDisabled = 0xc0000080,
            ServerNotDisabled = 0xc0000081,
            TooManyGuidsRequested = 0xc0000082,
            GuidsExhausted = 0xc0000083,
            InvalidIdAuthority = 0xc0000084,
            AgentsExhausted = 0xc0000085,
            InvalidVolumeLabel = 0xc0000086,
            SectionNotExtended = 0xc0000087,
            NotMappedData = 0xc0000088,
            ResourceDataNotFound = 0xc0000089,
            ResourceTypeNotFound = 0xc000008a,
            ResourceNameNotFound = 0xc000008b,
            ArrayBoundsExceeded = 0xc000008c,
            FloatDenormalOperand = 0xc000008d,
            FloatDivideByZero = 0xc000008e,
            FloatInexactResult = 0xc000008f,
            FloatInvalidOperation = 0xc0000090,
            FloatOverflow = 0xc0000091,
            FloatStackCheck = 0xc0000092,
            FloatUnderflow = 0xc0000093,
            IntegerDivideByZero = 0xc0000094,
            IntegerOverflow = 0xc0000095,
            PrivilegedInstruction = 0xc0000096,
            TooManyPagingFiles = 0xc0000097,
            FileInvalid = 0xc0000098,
            InstanceNotAvailable = 0xc00000ab,
            PipeNotAvailable = 0xc00000ac,
            InvalidPipeState = 0xc00000ad,
            PipeBusy = 0xc00000ae,
            IllegalFunction = 0xc00000af,
            PipeDisconnected = 0xc00000b0,
            PipeClosing = 0xc00000b1,
            PipeConnected = 0xc00000b2,
            PipeListening = 0xc00000b3,
            InvalidReadMode = 0xc00000b4,
            IoTimeout = 0xc00000b5,
            FileForcedClosed = 0xc00000b6,
            ProfilingNotStarted = 0xc00000b7,
            ProfilingNotStopped = 0xc00000b8,
            NotSameDevice = 0xc00000d4,
            FileRenamed = 0xc00000d5,
            CantWait = 0xc00000d8,
            PipeEmpty = 0xc00000d9,
            CantTerminateSelf = 0xc00000db,
            InternalError = 0xc00000e5,
            InvalidParameter1 = 0xc00000ef,
            InvalidParameter2 = 0xc00000f0,
            InvalidParameter3 = 0xc00000f1,
            InvalidParameter4 = 0xc00000f2,
            InvalidParameter5 = 0xc00000f3,
            InvalidParameter6 = 0xc00000f4,
            InvalidParameter7 = 0xc00000f5,
            InvalidParameter8 = 0xc00000f6,
            InvalidParameter9 = 0xc00000f7,
            InvalidParameter10 = 0xc00000f8,
            InvalidParameter11 = 0xc00000f9,
            InvalidParameter12 = 0xc00000fa,
            MappedFileSizeZero = 0xc000011e,
            TooManyOpenedFiles = 0xc000011f,
            Cancelled = 0xc0000120,
            CannotDelete = 0xc0000121,
            InvalidComputerName = 0xc0000122,
            FileDeleted = 0xc0000123,
            SpecialAccount = 0xc0000124,
            SpecialGroup = 0xc0000125,
            SpecialUser = 0xc0000126,
            MembersPrimaryGroup = 0xc0000127,
            FileClosed = 0xc0000128,
            TooManyThreads = 0xc0000129,
            ThreadNotInProcess = 0xc000012a,
            TokenAlreadyInUse = 0xc000012b,
            PagefileQuotaExceeded = 0xc000012c,
            CommitmentLimit = 0xc000012d,
            InvalidImageLeFormat = 0xc000012e,
            InvalidImageNotMz = 0xc000012f,
            InvalidImageProtect = 0xc0000130,
            InvalidImageWin16 = 0xc0000131,
            LogonServer = 0xc0000132,
            DifferenceAtDc = 0xc0000133,
            SynchronizationRequired = 0xc0000134,
            DllNotFound = 0xc0000135,
            IoPrivilegeFailed = 0xc0000137,
            OrdinalNotFound = 0xc0000138,
            EntryPointNotFound = 0xc0000139,
            ControlCExit = 0xc000013a,
            PortNotSet = 0xc0000353,
            DebuggerInactive = 0xc0000354,
            CallbackBypass = 0xc0000503,
            PortClosed = 0xc0000700,
            MessageLost = 0xc0000701,
            InvalidMessage = 0xc0000702,
            RequestCanceled = 0xc0000703,
            RecursiveDispatch = 0xc0000704,
            LpcReceiveBufferExpected = 0xc0000705,
            LpcInvalidConnectionUsage = 0xc0000706,
            LpcRequestsNotAllowed = 0xc0000707,
            ResourceInUse = 0xc0000708,
            ProcessIsProtected = 0xc0000712,
            VolumeDirty = 0xc0000806,
            FileCheckedOut = 0xc0000901,
            CheckOutRequired = 0xc0000902,
            BadFileType = 0xc0000903,
            FileTooLarge = 0xc0000904,
            FormsAuthRequired = 0xc0000905,
            VirusInfected = 0xc0000906,
            VirusDeleted = 0xc0000907,
            TransactionalConflict = 0xc0190001,
            InvalidTransaction = 0xc0190002,
            TransactionNotActive = 0xc0190003,
            TmInitializationFailed = 0xc0190004,
            RmNotActive = 0xc0190005,
            RmMetadataCorrupt = 0xc0190006,
            TransactionNotJoined = 0xc0190007,
            DirectoryNotRm = 0xc0190008,
            CouldNotResizeLog = 0xc0190009,
            TransactionsUnsupportedRemote = 0xc019000a,
            LogResizeInvalidSize = 0xc019000b,
            RemoteFileVersionMismatch = 0xc019000c,
            CrmProtocolAlreadyExists = 0xc019000f,
            TransactionPropagationFailed = 0xc0190010,
            CrmProtocolNotFound = 0xc0190011,
            TransactionSuperiorExists = 0xc0190012,
            TransactionRequestNotValid = 0xc0190013,
            TransactionNotRequested = 0xc0190014,
            TransactionAlreadyAborted = 0xc0190015,
            TransactionAlreadyCommitted = 0xc0190016,
            TransactionInvalidMarshallBuffer = 0xc0190017,
            CurrentTransactionNotValid = 0xc0190018,
            LogGrowthFailed = 0xc0190019,
            ObjectNoLongerExists = 0xc0190021,
            StreamMiniversionNotFound = 0xc0190022,
            StreamMiniversionNotValid = 0xc0190023,
            MiniversionInaccessibleFromSpecifiedTransaction = 0xc0190024,
            CantOpenMiniversionWithModifyIntent = 0xc0190025,
            CantCreateMoreStreamMiniversions = 0xc0190026,
            HandleNoLongerValid = 0xc0190028,
            NoTxfMetadata = 0xc0190029,
            LogCorruptionDetected = 0xc0190030,
            CantRecoverWithHandleOpen = 0xc0190031,
            RmDisconnected = 0xc0190032,
            EnlistmentNotSuperior = 0xc0190033,
            RecoveryNotNeeded = 0xc0190034,
            RmAlreadyStarted = 0xc0190035,
            FileIdentityNotPersistent = 0xc0190036,
            CantBreakTransactionalDependency = 0xc0190037,
            CantCrossRmBoundary = 0xc0190038,
            TxfDirNotEmpty = 0xc0190039,
            IndoubtTransactionsExist = 0xc019003a,
            TmVolatile = 0xc019003b,
            RollbackTimerExpired = 0xc019003c,
            TxfAttributeCorrupt = 0xc019003d,
            EfsNotAllowedInTransaction = 0xc019003e,
            TransactionalOpenNotAllowed = 0xc019003f,
            TransactedMappingUnsupportedRemote = 0xc0190040,
            TxfMetadataAlreadyPresent = 0xc0190041,
            TransactionScopeCallbacksNotSet = 0xc0190042,
            TransactionRequiredPromotion = 0xc0190043,
            CannotExecuteFileInTransaction = 0xc0190044,
            TransactionsNotFrozen = 0xc0190045,

            MaximumNtStatus = 0xffffffff
        }

        [DllImport("user32.dll")]
        internal static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        internal static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);

        internal enum DepSystemPolicyType
        {
            AlwaysOff = 0,
            AlwaysOn,
            OptIn,
            OptOut
        }

        [DllImport("kernel32.dll")]
        internal static extern int GetSystemDEPPolicy();

        [DllImport("user32.dll")]
        internal static extern int SendMessage(
            int hWnd, // handle to destination window
            uint Msg, // message
            int wParam, // first message parameter
            int lParam // second message parameter
        );

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal struct PEB_LDR_DATA
        {
            internal int Length;
            internal int Initialized;
            internal int SsHandle;
            internal IntPtr InLoadOrderModuleListPtr;
            internal IntPtr InMemoryOrderModuleListPtr;
            internal IntPtr InInitOrderModuleListPtr;
            internal int EntryInProgress;
            internal ListEntryWrapper InLoadOrderModuleList => InLoadOrderModuleListPtr.ReadAs<ListEntryWrapper>();
            internal ListEntryWrapper InMemoryOrderModuleList => InLoadOrderModuleListPtr.ReadAs<ListEntryWrapper>();
            internal ListEntryWrapper InInitOrderModuleList => InLoadOrderModuleListPtr.ReadAs<ListEntryWrapper>();
        }


        [StructLayout(LayoutKind.Sequential)]
        internal struct LDR_MODULE
        {
            internal ulong InMemoryOrderModuleList;
            internal ulong InInitializationOrderModuleList;
            internal IntPtr BaseAddress;
            internal IntPtr EntryPoint;
            internal uint SizeOfImage;
            internal uint space;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string FullDllName;
            internal uint _space;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string BaseDllName;
            internal ulong Flags;
            internal UInt16 LoadCount;
            internal UInt16 TlsIndex;
            internal ulong HashTableEntry;
            internal ulong TimeDateStamp;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal struct ListEntryWrapper
        {
            internal LIST_ENTRY Header;
            internal LDR_MODULE Body;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct LIST_ENTRY
        {
            public IntPtr Flink;
            public IntPtr Blink;

            public ListEntryWrapper Fwd
            {
                get
                {
                    var fwdAddr = Flink.ToInt32();
                    return new ListEntryWrapper()
                    {
                        Header = Flink.ReadAs<LIST_ENTRY>(),
                        Body = new IntPtr(fwdAddr + Marshal.SizeOf(typeof(LIST_ENTRY))).ReadAs<LDR_MODULE>()
                    };
                }
            }
            public ListEntryWrapper Back
            {
                get
                {
                    var bwAddr = Blink.ToInt32();
                    return new ListEntryWrapper()
                    {
                        Header = Flink.ReadAs<LIST_ENTRY>(),
                        Body = new IntPtr(bwAddr + Marshal.SizeOf(typeof(LIST_ENTRY))).ReadAs<LDR_MODULE>()
                    };
                }
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        internal struct IMAGE_FILE_HEADER
        {
            internal UInt32 mMagic; // PE\0\0 or 0x00004550
            internal UInt16 mMachine;
            internal UInt16 mNumberOfSections;
            internal UInt32 mTimeDateStamp;
            internal UInt32 mPointerToSymbolTable;
            internal UInt32 mNumberOfSymbols;
            internal UInt16 mSizeOfOptionalHeader;
            internal UInt16 mCharacteristics;
        }

        internal enum MachineType : ushort
        {
            Native = 0,
            I386 = 0x014c,
            Itanium = 0x0200,
            x64 = 0x8664
        }
        internal enum MagicType : ushort
        {
            IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b,
            IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b
        }
        internal enum SubSystemType : ushort
        {
            IMAGE_SUBSYSTEM_UNKNOWN = 0,
            IMAGE_SUBSYSTEM_NATIVE = 1,
            IMAGE_SUBSYSTEM_WINDOWS_GUI = 2,
            IMAGE_SUBSYSTEM_WINDOWS_CUI = 3,
            IMAGE_SUBSYSTEM_POSIX_CUI = 7,
            IMAGE_SUBSYSTEM_WINDOWS_CE_GUI = 9,
            IMAGE_SUBSYSTEM_EFI_APPLICATION = 10,
            IMAGE_SUBSYSTEM_EFI_BOOT_SERVICE_DRIVER = 11,
            IMAGE_SUBSYSTEM_EFI_RUNTIME_DRIVER = 12,
            IMAGE_SUBSYSTEM_EFI_ROM = 13,
            IMAGE_SUBSYSTEM_XBOX = 14

        }
        internal enum DllCharacteristicsType : ushort
        {
            RES_0 = 0x0001,
            RES_1 = 0x0002,
            RES_2 = 0x0004,
            RES_3 = 0x0008,
            IMAGE_DLL_CHARACTERISTICS_DYNAMIC_BASE = 0x0040,
            IMAGE_DLL_CHARACTERISTICS_FORCE_INTEGRITY = 0x0080,
            IMAGE_DLL_CHARACTERISTICS_NX_COMPAT = 0x0100,
            IMAGE_DLLCHARACTERISTICS_NO_ISOLATION = 0x0200,
            IMAGE_DLLCHARACTERISTICS_NO_SEH = 0x0400,
            IMAGE_DLLCHARACTERISTICS_NO_BIND = 0x0800,
            RES_4 = 0x1000,
            IMAGE_DLLCHARACTERISTICS_WDM_DRIVER = 0x2000,
            IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE = 0x8000
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct IMAGE_DATA_DIRECTORY
        {
            internal UInt32 VirtualAddress;
            internal UInt32 Size;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct IMAGE_OPTIONAL_HEADER32
        {
            [FieldOffset(0)]
            internal MagicType Magic;

            [FieldOffset(2)]
            internal byte MajorLinkerVersion;

            [FieldOffset(3)]
            internal byte MinorLinkerVersion;

            [FieldOffset(4)]
            internal uint SizeOfCode;

            [FieldOffset(8)]
            internal uint SizeOfInitializedData;

            [FieldOffset(12)]
            internal uint SizeOfUninitializedData;

            [FieldOffset(16)]
            internal uint AddressOfEntryPoint;

            [FieldOffset(20)]
            internal uint BaseOfCode;

            // PE32 contains this additional field
            [FieldOffset(24)]
            internal uint BaseOfData;

            [FieldOffset(28)]
            internal uint ImageBase;

            [FieldOffset(32)]
            internal uint SectionAlignment;

            [FieldOffset(36)]
            internal uint FileAlignment;

            [FieldOffset(40)]
            internal ushort MajorOperatingSystemVersion;

            [FieldOffset(42)]
            internal ushort MinorOperatingSystemVersion;

            [FieldOffset(44)]
            internal ushort MajorImageVersion;

            [FieldOffset(46)]
            internal ushort MinorImageVersion;

            [FieldOffset(48)]
            internal ushort MajorSubsystemVersion;

            [FieldOffset(50)]
            internal ushort MinorSubsystemVersion;

            [FieldOffset(52)]
            internal uint Win32VersionValue;

            [FieldOffset(56)]
            internal uint SizeOfImage;

            [FieldOffset(60)]
            internal uint SizeOfHeaders;

            [FieldOffset(64)]
            internal uint CheckSum;

            [FieldOffset(68)]
            internal SubSystemType Subsystem;

            [FieldOffset(70)]
            internal DllCharacteristicsType DllCharacteristics;

            [FieldOffset(72)]
            internal uint SizeOfStackReserve;

            [FieldOffset(76)]
            internal uint SizeOfStackCommit;

            [FieldOffset(80)]
            internal uint SizeOfHeapReserve;

            [FieldOffset(84)]
            internal uint SizeOfHeapCommit;

            [FieldOffset(88)]
            internal uint LoaderFlags;

            [FieldOffset(92)]
            internal uint NumberOfRvaAndSizes;

            [FieldOffset(96)]
            internal IMAGE_DATA_DIRECTORY ExportTable;

            [FieldOffset(104)]
            internal IMAGE_DATA_DIRECTORY ImportTable;

            [FieldOffset(112)]
            internal IMAGE_DATA_DIRECTORY ResourceTable;

            [FieldOffset(120)]
            internal IMAGE_DATA_DIRECTORY ExceptionTable;

            [FieldOffset(128)]
            internal IMAGE_DATA_DIRECTORY CertificateTable;

            [FieldOffset(136)]
            internal IMAGE_DATA_DIRECTORY BaseRelocationTable;

            [FieldOffset(144)]
            internal IMAGE_DATA_DIRECTORY Debug;

            [FieldOffset(152)]
            internal IMAGE_DATA_DIRECTORY Architecture;

            [FieldOffset(160)]
            internal IMAGE_DATA_DIRECTORY GlobalPtr;

            [FieldOffset(168)]
            internal IMAGE_DATA_DIRECTORY TLSTable;

            [FieldOffset(176)]
            internal IMAGE_DATA_DIRECTORY LoadConfigTable;

            [FieldOffset(184)]
            internal IMAGE_DATA_DIRECTORY BoundImport;

            [FieldOffset(192)]
            internal IMAGE_DATA_DIRECTORY IAT;

            [FieldOffset(200)]
            internal IMAGE_DATA_DIRECTORY DelayImportDescriptor;

            [FieldOffset(208)]
            internal IMAGE_DATA_DIRECTORY CLRRuntimeHeader;

            [FieldOffset(216)]
            internal IMAGE_DATA_DIRECTORY Reserved;
        }




        [StructLayout(LayoutKind.Sequential)]
        internal struct IMAGE_DOS_HEADER
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            internal char[] e_magic;       // Magic number
            internal UInt16 e_cblp;    // Bytes on last page of file
            internal UInt16 e_cp;      // Pages in file
            internal UInt16 e_crlc;    // Relocations
            internal UInt16 e_cparhdr;     // Size of header in paragraphs
            internal UInt16 e_minalloc;    // Minimum extra paragraphs needed
            internal UInt16 e_maxalloc;    // Maximum extra paragraphs needed
            internal UInt16 e_ss;      // Initial (relative) SS value
            internal UInt16 e_sp;      // Initial SP value
            internal UInt16 e_csum;    // Checksum
            internal UInt16 e_ip;      // Initial IP value
            internal UInt16 e_cs;      // Initial (relative) CS value
            internal UInt16 e_lfarlc;      // File address of relocation table
            internal UInt16 e_ovno;    // Overlay number
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            internal UInt16[] e_res1;    // Reserved words
            internal UInt16 e_oemid;       // OEM identifier (for e_oeminfo)
            internal UInt16 e_oeminfo;     // OEM information; e_oemid specific
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            internal UInt16[] e_res2;    // Reserved words
            internal Int32 e_lfanew;      // File address of new exe header

            private string _e_magic
            {
                get { return new string(e_magic); }
            }

            internal bool isValid
            {
                get { return _e_magic == "MZ"; }
            }
        }

        internal enum Protection
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize,
        Protection flNewProtect, out Protection lpflOldProtect);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool PostMessage(int hWnd, uint Msg, int wParam, int lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        ///     Module32First
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        /// <summary>
        ///     Module32Next
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        /// <summary>
        ///     Get address of a function from a dll loaded inside the process (handle)
        /// </summary>
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        /// <summary>
        ///     Get Module handle for a loaded dll
        /// </summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("Kernel32")]
        internal static extern void AllocConsole();

        [DllImport("Kernel32")]
        internal static extern void FreeConsole();

        [DllImport("kernel32.dll")]
        internal static extern bool CreateProcess(string lpApplicationName,
            string lpCommandLine, IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles, ProcessCreationFlags dwCreationFlags,
            IntPtr lpEnvironment, string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll")]
        internal static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        internal static extern uint SuspendThread(IntPtr hThread);

        [DllImport("User32.dll")]
        internal static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern void SetLastError(uint dwErrCode);


        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        internal static extern int CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int Msg, int wParam, int lParam);


        [DllImport("winmm.dll", SetLastError = true)]
        internal static extern bool PlaySound(byte[] pszSound, IntPtr hmod, SoundFlags fdwSound);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("kernel32.dll")]
        internal static extern uint GetCurrentThreadId();

        /// <summary>
        ///     MODULEENTRY32 struct for Module32First/Next
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MODULEENTRY32
        {
            internal uint dwSize;
            internal uint th32ModuleID;
            internal uint th32ProcessID;
            internal uint GlblcntUsage;
            internal uint ProccntUsage;
            private readonly IntPtr modBaseAddr;
            internal uint modBaseSize;
            private readonly IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] internal string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] internal string szExePath;
        }

        internal struct SYSTEM_INFO
        {
            internal ushort processorArchitecture;
#pragma warning disable 169
            private ushort reserved;
#pragma warning restore 169
            internal uint pageSize;
            internal IntPtr minimumApplicationAddress; // minimum address
            internal IntPtr maximumApplicationAddress; // maximum address
            internal IntPtr activeProcessorMask;
            internal uint numberOfProcessors;
            internal uint processorType;
            internal uint allocationGranularity;
            internal ushort processorLevel;
            internal ushort processorRevision;
        }

        internal struct STARTUPINFO
        {
            internal uint cb;
            internal string lpReserved;
            internal string lpDesktop;
            internal string lpTitle;
            internal uint dwX;
            internal uint dwY;
            internal uint dwXSize;
            internal uint dwYSize;
            internal uint dwXCountChars;
            internal uint dwYCountChars;
            internal uint dwFillAttribute;
            internal uint dwFlags;
            internal short wShowWindow;
            internal short cbReserved2;
            internal IntPtr lpReserved2;
            internal IntPtr hStdInput;
            internal IntPtr hStdOutput;
            internal IntPtr hStdError;
        }

        internal struct PROCESS_INFORMATION
        {
            internal IntPtr hProcess;
            internal IntPtr hThread;
            internal uint dwProcessId;
            internal uint dwThreadId;
        }

        [Flags]
        internal enum ProcessCreationFlags : uint
        {
            ZERO_FLAG = 0x00000000,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00001000,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            INHERIT_PARENT_AFFINITY = 0x00010000
        }


        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate int WindowProc(IntPtr hWnd, int Msg, int wParam, int lParam);


        [Flags]
        internal enum SoundFlags
        {
            /// <summary>play synchronously (default)</summary>
            SND_SYNC = 0x0000,

            /// <summary>play asynchronously</summary>
            SND_ASYNC = 0x0001,

            /// <summary>silence (!default) if sound not found</summary>
            SND_NODEFAULT = 0x0002,

            /// <summary>pszSound points to a memory file</summary>
            SND_MEMORY = 0x0004,

            /// <summary>loop the sound until next sndPlaySound</summary>
            SND_LOOP = 0x0008,

            /// <summary>don't stop any currently playing sound</summary>
            SND_NOSTOP = 0x0010,

            /// <summary>Stop Playing Wave</summary>
            SND_PURGE = 0x40,

            /// <summary>don't wait if the driver is busy</summary>
            SND_NOWAIT = 0x00002000,

            /// <summary>name is a registry alias</summary>
            SND_ALIAS = 0x00010000,

            /// <summary>alias is a predefined id</summary>
            SND_ALIAS_ID = 0x00110000,

            /// <summary>name is file name</summary>
            SND_FILENAME = 0x00020000,

            /// <summary>name is resource name or atom</summary>
            SND_RESOURCE = 0x00040004
        }

        internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    }
}
