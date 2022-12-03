using Binarysharp.Assemblers.Fasm;
using BloogBot.Game;
using BloogBot.Game.Cache;
using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;

namespace BloogBot
{
    public static unsafe class MemoryManager
    {
        [Flags]
        enum ProcessAccessFlags
        {
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            SYNCHRONIZE = 0x00100000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            PROCESS_ALL_ACCESS = 0x001F0FFF,
            PROCESS_CREATE_PROCESS = 0x0080,
            PROCESS_CREATE_THREAD = 0x0002,
            PROCESS_DUP_HANDLE = 0x0040,
            PROCESS_QUERY_INFORMATION = 0x0400,
            PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
            PROCESS_SET_INFORMATION = 0x0200,
            PROCESS_SET_QUOTA = 0x0100,
            PROCESS_SUSPEND_RESUME = 0x0800,
            PROCESS_TERMINATE = 0x0001,
            PROCESS_VM_OPERATION = 0x0008,
            PROCESS_VM_READ = 0x0010,
            PROCESS_VM_WRITE = 0x0020
        }

        [DllImport("kernel32.dll")]
        static extern bool VirtualProtect(IntPtr address, int size, uint newProtect, out uint oldProtect);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccessFlags desiredAccess, bool inheritHandle, int processId);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int dwSize,
            ref int lpNumberOfBytesWritten);

        [Flags]
        public enum Protection
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

        [DllImport("kernel32.dll")]
        static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        static readonly IntPtr wowProcessHandle = Process.GetCurrentProcess().Handle;
        static readonly FasmNet fasm = new FasmNet();

        [HandleProcessCorruptedStateExceptions]
        static internal byte ReadByte(IntPtr address)
        {
            if (address == IntPtr.Zero)
                return 0;

            try
            {
                return *(byte*)address;
            }
            catch (AccessViolationException)
            {
                Logger.Log("Access Violation on " + address.ToString("X") + " with type Byte");
                return default;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        static public int ReadInt(IntPtr address)
        {
            if (address == IntPtr.Zero)
                return 0;

            try
            {
                return *(int*)address;
            }
            catch (AccessViolationException)
            {
                Logger.Log("Access Violation on " + address.ToString("X") + " with type Int");
                return default;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        static public uint ReadUint(IntPtr address)
        {
            if (address == IntPtr.Zero)
                return 0;

            try
            {
                return *(uint*)address;
            }
            catch (AccessViolationException)
            {
                Logger.Log("Access Violation on " + address.ToString("X") + " with type Uint");
                return default;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        static public ulong ReadUlong(IntPtr address)
        {
            if (address == IntPtr.Zero)
                return 0;

            try
            {
                return *(ulong*)address;
            }
            catch (AccessViolationException)
            {
                Logger.Log("Access Violation on " + address.ToString("X") + " with type Ulong");
                return default;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        static public IntPtr ReadIntPtr(IntPtr address)
        {
            if (address == IntPtr.Zero)
                return IntPtr.Zero;

            try
            {
                return *(IntPtr*)address;
            }
            catch (AccessViolationException)
            {
                Logger.Log("Access Violation on " + address.ToString("X") + " with type IntPtr");
                return default;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        static public float ReadFloat(IntPtr address)
        {
            if (address == IntPtr.Zero)
                return 0;

            try
            {
                return *(float*)address;
            }
            catch (AccessViolationException)
            {
                Logger.Log("Access Violation on " + address.ToString("X") + " with type Float");
                return default;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        static public string ReadString(IntPtr address, int size = 512)
        {
            if (address == IntPtr.Zero)
                return null;

            try
            {
                var buffer = ReadBytes(address, size);
                if (buffer.Length == 0)
                    return default;

                var ret = Encoding.ASCII.GetString(buffer);

                if (ret.IndexOf('\0') != -1)
                    ret = ret.Remove(ret.IndexOf('\0'));

                return ret;
            }
            catch (AccessViolationException)
            {
                Logger.Log("Access Violation on " + address.ToString("X") + " with type string");
                return default;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        static public byte[] ReadBytes(IntPtr address, int count)
        {
            if (address == IntPtr.Zero)
                return null;

            try
            {
                var ret = new byte[count];
                var ptr = (byte*)address;

                for (var i = 0; i < count; i++)
                    ret[i] = ptr[i];

                return ret;
            }
            catch (NullReferenceException)
            {
                return default;
            }
            catch (AccessViolationException)
            {
                return default;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        static public ItemCacheEntry ReadItemCacheEntry(IntPtr address)
        {
            if (address == IntPtr.Zero)
                return null;

            try
            {
                return new ItemCacheEntry(address);
            }
            catch (AccessViolationException)
            {
                Logger.Log("Access Violation on " + address.ToString("X") + " with type ItemCacheEntry");
                return default;
            }
        }

        static internal void WriteByte(IntPtr address, byte value) => Marshal.StructureToPtr(value, address, false);

        static internal void WriteInt(IntPtr address, int value) => Marshal.StructureToPtr(value, address, false);

        // certain memory locations (Warden for example) are protected from modification.
        // we use OpenAccess with ProcessAccessFlags to remove the protection.
        // you can check whether memory is successfully being modified by setting a breakpoint
        // here and checking Debug -> Windows -> Disassembly.
        // if you have further issues, you may need to use VirtualProtect from the Win32 API.
        static internal void WriteBytes(IntPtr address, byte[] bytes)
        {
            if (address == IntPtr.Zero)
                return;

            var access = ProcessAccessFlags.PROCESS_CREATE_THREAD |
                         ProcessAccessFlags.PROCESS_QUERY_INFORMATION |
                         ProcessAccessFlags.PROCESS_SET_INFORMATION |
                         ProcessAccessFlags.PROCESS_TERMINATE |
                         ProcessAccessFlags.PROCESS_VM_OPERATION |
                         ProcessAccessFlags.PROCESS_VM_READ |
                         ProcessAccessFlags.PROCESS_VM_WRITE |
                         ProcessAccessFlags.SYNCHRONIZE;

            var process = OpenProcess(access, false, Process.GetCurrentProcess().Id);

            int ret = 0;
            WriteProcessMemory(process, address, bytes, bytes.Length, ref ret);

            var protection = Protection.PAGE_EXECUTE_READWRITE;
            // now set the memory to be executable
            VirtualProtect(address, bytes.Length, (uint)protection, out uint _);
        }

        static internal IntPtr InjectAssembly(string hackName, string[] instructions)
        {
            // first get the assembly as bytes for the allocated area before overwriting the memory
            fasm.Clear();
            fasm.AddLine("use32");
            foreach (var x in instructions)
                fasm.AddLine(x);

            var byteCode = new byte[0];
            try
            {
                byteCode = fasm.Assemble();
            }
            catch (FasmAssemblerException ex)
            {
                Logger.Log(ex);
            }

            var start = Marshal.AllocHGlobal(byteCode.Length);
            fasm.Clear();
            fasm.AddLine("use32");
            foreach (var x in instructions)
                fasm.AddLine(x);
            byteCode = fasm.Assemble(start);

            var hack = new Hack(hackName, start, byteCode);
            HackManager.AddHack(hack);

            return start;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
        }

        static internal void InjectAssembly(string hackName, uint ptr, string instructions)
        {
            fasm.Clear();
            fasm.AddLine("use32");
            fasm.AddLine(instructions);
            var start = new IntPtr(ptr);
            var byteCode = fasm.Assemble(start);

            var hack = new Hack(hackName, start, byteCode);
            HackManager.AddHack(hack);
        }
    }
}
