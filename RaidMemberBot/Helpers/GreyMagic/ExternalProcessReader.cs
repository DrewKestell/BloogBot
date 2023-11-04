using RaidMemberBot.Helpers.GreyMagic.Internals;
using RaidMemberBot.Helpers.GreyMagic.Native;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

namespace RaidMemberBot.Helpers.GreyMagic
{
    internal sealed unsafe class ExternalProcessReader : InProcessMemoryReader
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ExternalProcessReader" /> class.
        /// </summary>
        /// <param name="proc">The proc.</param>
        /// <remarks>Created 2012-04-24</remarks>
        internal ExternalProcessReader(Process proc) : base(proc)
        {
            if (IsProcessOpen)
            {
                ThreadHandle = Imports.OpenThread(0x0001F03FF, false, (uint)proc.Threads[0].Id);
                WindowHandle = Process.MainWindowHandle;
            }
            else
            {
                throw new Exception("ProcessHandle is invalid or closed, are you sure you did everything right?");
            }
        }

        /// <summary>
        ///     This is not valid in this memory reader type.
        /// </summary>
        internal override DetourManager Detours
        {
            get { return null; }
        }

        /// <summary>
        ///     Gets the thread handle.
        /// </summary>
        /// <remarks>Created 2012-04-23</remarks>
        internal IntPtr ThreadHandle { get; }

        /// <summary>
        ///     Gets the window handle.
        /// </summary>
        /// <remarks>Created 2012-04-23</remarks>
        internal IntPtr WindowHandle { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the main thread is open.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the main thread is open; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Created 2012-04-23</remarks>
        internal bool IsThreadOpen
        {
            get { return ThreadHandle != IntPtr.Zero; }
        }

        /// <summary>
        ///     Gets a value indicating whether the process is open for memory manipulation.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the process is open for memory manipulation; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Created 2012-04-23</remarks>
        internal bool IsProcessOpen
        {
            get { return ProcessHandle != null && !ProcessHandle.IsClosed && !ProcessHandle.IsInvalid; }
        }

        /// <summary>
        ///     Gets the module loaded by the opened process that matches the given string.
        /// </summary>
        /// <param name="sModuleName">String specifying which module to return.</param>
        /// <returns>Returns the module loaded by the opened process that matches the given string.</returns>
        internal ProcessModule GetModule(string sModuleName)
        {
            return
                Process.Modules.Cast<ProcessModule>().FirstOrDefault(
                    pMod => pMod.ModuleName.ToLower().Equals(sModuleName.ToLower()));
        }

        #region Imports

        [DllImport("kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool ReadProcessMemory(SafeMemoryHandle hProcess, IntPtr lpBaseAddress, byte* lpBuffer,
            int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool WriteProcessMemory(SafeMemoryHandle hProcess, IntPtr lpBaseAddress, byte[] lpBuffer,
            int nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool ReadProcessMemory(SafeMemoryHandle hProcess, uint dwAddress, IntPtr lpBuffer,
            int nSize, out int lpBytesRead);

        [DllImport("kernel32.dll")]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool VirtualProtectEx(SafeMemoryHandle hProcess, IntPtr lpAddress, IntPtr dwSize,
            uint flNewProtect, out uint lpflOldProtect);

        #endregion

        #region Overrides of MemoryBase

        /// <summary>
        ///     Reads a specific number of bytes from memory.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="count">The count.</param>
        /// <param name="isRelative"></param>
        /// <returns></returns>
        internal override byte[] ReadBytes(IntPtr address, int count, bool isRelative = false)
        {
            // RPM allows 0 bytes to be read. Assuming it tosses back true immediately without filling the buffer.
            // Avoid the overhead of calling it, and return back a 0 entry array.
            if (count == 0)
                return new byte[0];

            // Yes, this *can* be valid. But in 99.9999% of cases, its not. If it ever is, you should be smart enough
            // to remove this check.
            if (address == IntPtr.Zero)
                throw new ArgumentException("Address cannot be zero.", "address");

            // Rebase
            if (isRelative)
                address = GetAbsolute(address);

            byte[] buffer = new byte[count];
            fixed (byte* buf = buffer)
            {
                int numRead;
                if (ReadProcessMemory(ProcessHandle, address, buf, count, out numRead) && numRead == count)
                    return buffer;
            }

            throw new AccessViolationException(string.Format("Could not read bytes from {0} [{1}]!",
                isRelative ? GetRelative(address).ToString("X8") : address.ToString("X8"), Marshal.GetLastWin32Error()));
        }

        /// <summary>
        ///     Writes a set of bytes to memory.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="bytes">The bytes.</param>
        /// <param name="isRelative">if set to <c>true</c> [is relative].</param>
        /// <returns>
        ///     Number of bytes written.
        /// </returns>
        internal override int WriteBytes(IntPtr address, byte[] bytes, bool isRelative = false)
        {
            if (isRelative)
                address = GetAbsolute(address);

            int numWritten;
            bool success = WriteProcessMemory(ProcessHandle, address, bytes, bytes.Length, out numWritten);

            if (!success || numWritten != bytes.Length)
                throw new AccessViolationException(string.Format(
                    "Could not write the specified bytes! {0} to {1} [{2}]", bytes.Length, address.ToString("X8"),
                    new Win32Exception(Marshal.GetLastWin32Error()).Message));

            return numWritten;
        }

        /// <summary>
        ///     Reads a specific number of bytes from memory.
        /// </summary>
        /// <param name="dwAddress">The address.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// <remarks>Created 2012-04-23</remarks>
        internal new int ReadBytes(uint dwAddress, void* buffer, int count)
        {
            int lpBytesRead;
            if (!ReadProcessMemory(ProcessHandle, dwAddress, new IntPtr(buffer), count, out lpBytesRead))
                throw new AccessViolationException(string.Format("Could not read {2} byte(s) from {0} [{1}]!",
                    dwAddress.ToString("X8"), Marshal.GetLastWin32Error(), count));

            return lpBytesRead;
        }

        /// <summary>
        ///     Reads a specific number of bytes from memory and writes them to an unsafe address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="buffer"></param>
        /// <param name="count"></param>
        internal new void ReadUnsafe(IntPtr address, void* buffer, int count)
        {
            if (ReadBytes((uint)address, buffer, count) != count)
                throw new Exception("Exception while reading " + count + " bytes from " + ((uint)address).ToString("X"));
        }

        /// <summary> Reads a value from the specified address in memory. </summary>
        /// <remarks> Created 3/26/2012. </remarks>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="address"> The address. </param>
        /// <param name="isRelative"> (optional) the relative. </param>
        /// <returns> . </returns>
        internal override T Read<T>(IntPtr address, bool isRelative = false)
        {
            fixed (byte* b = ReadBytes(address, MarshalCache<T>.Size, isRelative))
            {
                return base.Read<T>((IntPtr)b);
            }
        }

        internal override T[] Read<T>(IntPtr address, int count, bool isRelative = false)
        {
            T[] ret = new T[count];
            int size = MarshalCache<T>.Size;
            fixed (byte* buffer = ReadBytes(address, size * count, isRelative))
            {
                for (int i = 0; i < count; i++)
                    ret[i] = base.Read<T>((IntPtr)(buffer + i * size));
            }
            return ret;
        }

        /// <summary> Writes a value specified to the address in memory. </summary>
        /// <remarks> Created 3/26/2012. </remarks>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="address"> The address. </param>
        /// <param name="value"> The value. </param>
        /// <param name="isRelative"> (optional) the relative. </param>
        /// <returns> true if it succeeds, false if it fails. </returns>
        internal override bool Write<T>(IntPtr address, T value, bool isRelative = false)
        {
            if (isRelative)
                address = GetAbsolute(address);

            byte[] buffer;
            IntPtr hObj = Marshal.AllocHGlobal(MarshalCache<T>.Size);
            try
            {
                Marshal.StructureToPtr(value, hObj, false);

                buffer = new byte[MarshalCache<T>.Size];
                Marshal.Copy(hObj, buffer, 0, MarshalCache<T>.Size);
            }
            finally
            {
                Marshal.FreeHGlobal(hObj);
            }

            int numWritten;
            uint oldProtect;
            // Fix the protection flags to EXECUTE_READWRITE to ensure we're not going to cause issues.
            // make sure we put back the old protection when we're done!
            // dwSize should be IntPtr or UIntPtr because the underlying type is SIZE_T and varies with the platform.
            VirtualProtectEx(ProcessHandle, address, (IntPtr)MarshalCache<T>.Size, 0x40, out oldProtect);
            bool ret = WriteProcessMemory(ProcessHandle, address, buffer, MarshalCache<T>.Size, out numWritten);
            VirtualProtectEx(ProcessHandle, address, (IntPtr)MarshalCache<T>.Size, oldProtect, out oldProtect);

            return ret;
        }

        ~ExternalProcessReader()
        {
            ProcessHandle.Dispose();
            ProcessHandle = null;
            SafeMemoryHandle.CloseHandle(ThreadHandle);
        }

        #endregion

        #region Alloc

        /// <summary> Creates an allocated memory chunk. </summary>
        /// <remarks> Created 3/26/2012. </remarks>
        /// <param name="numBytes"> Number of bytes. </param>
        /// <returns> . </returns>
        internal AllocatedMemory CreateAllocatedMemory(int numBytes)
        {
            return new AllocatedMemory(this, numBytes);
        }

        /// <summary>
        ///     Frees an allocated block of memory in the opened process.
        /// </summary>
        /// <param name="address">Base address of the block of memory to be freed.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        /// <remarks>
        ///     Frees a block of memory using <see cref="MemoryFreeType.MEM_RELEASE" />.
        /// </remarks>
        internal bool FreeMemory(IntPtr address)
        {
            return FreeMemory(address, /*size must be 0 for MEM_RELEASE*/ 0, MemoryFreeType.MEM_RELEASE);
        }

        /// <summary>
        ///     Frees an allocated block of memory in the opened process.
        /// </summary>
        /// <param name="address">Base address of the block of memory to be freed.</param>
        /// <param name="size">
        ///     Number of bytes to be freed.  This must be zero (0) if using
        ///     <see cref="MemoryFreeType.MEM_RELEASE" />.
        /// </param>
        /// <param name="freeType">Type of free operation to use.  See <see cref="MemoryFreeType" />.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        internal bool FreeMemory(IntPtr address, int size, MemoryFreeType freeType)
        {
            if (freeType == MemoryFreeType.MEM_RELEASE)
                size = 0;

            return Imports.VirtualFreeEx(ProcessHandle, address, size, freeType);
        }

        /// <summary>
        ///     Allocates memory inside the opened process.
        /// </summary>
        /// <param name="size">Number of bytes to allocate.</param>
        /// <param name="allocationType">Type of memory allocation.  See <see cref="MemoryAllocationType" />.</param>
        /// <param name="protect">Type of memory protection.  See <see cref="MemoryProtectionType" /></param>
        /// <returns>Returns NULL on failure, or the base address of the allocated memory on success.</returns>
        internal IntPtr AllocateMemory(int size, MemoryAllocationType allocationType = MemoryAllocationType.MEM_COMMIT,
            MemoryProtectionType protect = MemoryProtectionType.PAGE_EXECUTE_READWRITE)
        {
            return Imports.VirtualAllocEx(ProcessHandle, 0, size, allocationType, protect);
        }

        #endregion
    }
}
