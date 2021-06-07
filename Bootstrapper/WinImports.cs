using System;
using System.Runtime.InteropServices;

namespace Bootstrapper
{
    static class WinImports
    {
        [DllImport("kernel32.dll")]
        internal static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            ProcessCreationFlag dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr dwAddress,
            int nSize,
            MemoryAllocationType dwAllocationType,
            MemoryProtectionType dwProtect);

        [DllImport("kernel32.dll")]
        internal static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int dwSize,
            ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttribute,
            IntPtr dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        internal static extern bool VirtualFreeEx(
            IntPtr hProcess,
            IntPtr dwAddress,
            int nSize,
            MemoryFreeType dwFreeType);

        internal enum MemoryAllocationType
        {
            MEM_COMMIT = 0x1000
        }

        internal enum MemoryProtectionType
        {
            PAGE_EXECUTE_READWRITE = 0x40
        }

        internal enum MemoryFreeType
        {
            MEM_RELEASE = 0x8000
        }

        internal enum ProcessCreationFlag
        {
            CREATE_DEFAULT_ERROR_MODE = 0x04000000
        }

        internal struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }
    }
}
