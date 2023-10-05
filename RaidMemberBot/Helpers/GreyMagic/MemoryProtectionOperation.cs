using System;
using System.Runtime.InteropServices;

namespace RaidMemberBot.Helpers.GreyMagic
{
    /// <summary>
    ///     Can be used to temporarily set Memory protection constants on a page.
    /// </summary>
    /// <remarks>Created 2012-01-16 16:33 by Nesox.</remarks>
    internal class MemoryProtectionOperation : IDisposable
    {
        private readonly IntPtr _address;
        private readonly SafeMemoryHandle _hProcess;
        private readonly uint _oldProtect;
        private readonly int _size;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MemoryProtectionOperation" /> class.
        /// </summary>
        /// <param name="hProcess">The process handle.</param>
        /// <param name="address">The address.</param>
        /// <param name="size">The size.</param>
        /// <param name="flNewProtect">The fl new protect.</param>
        /// <remarks>
        ///     Created 2012-01-16 16:34 by Nesox.
        /// </remarks>
        internal MemoryProtectionOperation(SafeMemoryHandle hProcess, IntPtr address, int size, uint flNewProtect)
        {
            _hProcess = hProcess;
            _address = address;
            _size = size;
#if OOP
            VirtualProtectEx(_hProcess, _address, size, flNewProtect, out _oldProtect);
#else
            VirtualProtect(_address, size, flNewProtect, out _oldProtect);
#endif
        }

        #region Implementation of IDisposable

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            uint trash;
#if OOP
            VirtualProtectEx(_hProcess, _address, _size, _oldProtect, out trash);
#else
            VirtualProtect(_address, _size, _oldProtect, out trash);
#endif
        }

        #endregion

        /// <summary>
        ///     Virtuals the protect.
        /// </summary>
        /// <param name="lpAddress">The lp address.</param>
        /// <param name="dwSize">Size of the dw.</param>
        /// <param name="flNewProtect">The fl new protect.</param>
        /// <param name="lpflOldProtect">The LPFL old protect.</param>
        /// <returns></returns>
        /// <remarks>Created 2012-01-16 16:34 by Nesox.</remarks>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualProtect(IntPtr lpAddress, int dwSize, uint flNewProtect,
            out uint lpflOldProtect);

        /// <summary>
        ///     Virtuals the protect.
        /// </summary>
        /// <param name="hProcess">The process handle.</param>
        /// <param name="lpAddress">The lp address.</param>
        /// <param name="dwSize">Size of the dw.</param>
        /// <param name="flNewProtect">The fl new protect.</param>
        /// <param name="lpflOldProtect">The LPFL old protect.</param>
        /// <returns></returns>
        /// <remarks>Created 2012-01-16 16:34 by Nesox.</remarks>
        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flNewProtect,
            out uint lpflOldProtect);
    }
}
