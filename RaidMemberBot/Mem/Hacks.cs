using RaidMemberBot.Game.Statics;
using System;
using System.Linq;

namespace RaidMemberBot.Mem
{
    /// <summary>
    ///     Class for a simple hack (read: changing bytes in memory)
    /// </summary>
    internal class Hack
    {
        // address where the bytes will be changed
        private IntPtr _address = IntPtr.Zero;

        // old bytes
        private byte[] _originalBytes;

        /// <summary>
        ///     Constructor: addr and the new bytes
        /// </summary>
        internal Hack(IntPtr parAddress, byte[] parCustomBytes, string parName)
        {
            Address = parAddress;
            _customBytes = parCustomBytes;
            _originalBytes = Memory.Reader.ReadBytes(Address, _customBytes.Length);
            Name = parName;
        }

        /// <summary>
        ///     Constructor: addr, new bytes aswell old bytes
        /// </summary>
        internal Hack(IntPtr parAddress, byte[] parCustomBytes, byte[] parOriginalBytes, string parName)
        {
            Address = parAddress;
            _customBytes = parCustomBytes;
            _originalBytes = parOriginalBytes;
            Name = parName;
        }

        internal Hack(uint offset, byte[] parCustomBytes, string parName)
        {
            _address = (IntPtr)offset;
            _customBytes = parCustomBytes;
            Name = parName;
        }

        internal bool DynamicHide { get; set; } = false;
        // is the hack applied
        //internal bool IsApplied { get; private set; }

        internal bool RelativeToPlayerBase { get; set; } = false;

        internal IntPtr Address
        {
            get
            {
                return !RelativeToPlayerBase
                    ? _address
                    : IntPtr.Add(ObjectManager.Instance.Player.Pointer, (int)_address);
            }
            private set { _address = value; }
        }

        // new bytes
        private byte[] _customBytes { get; set; }
        // name of hack
        internal string Name { get; private set; }

        internal bool IsActivated
        {
            get
            {
                if (RelativeToPlayerBase)
                {
                    if (!ObjectManager.Instance.IsIngame) return false;
                    if (ObjectManager.Instance.Player == null) return false;
                }
                byte[] curBytes = Memory.Reader.ReadBytes(Address, _originalBytes.Length);
                return !curBytes.SequenceEqual(_originalBytes);
            }
        }

        internal bool IsWithinScan(IntPtr scanStartAddress, int size)
        {
            int scanStart = (int)scanStartAddress;
            int scanEnd = (int)IntPtr.Add(scanStartAddress, size);

            int hackStart = (int)Address;
            int hackEnd = (int)Address + _customBytes.Length;

            if (hackStart >= scanStart && hackStart < scanEnd)
                return true;

            if (hackEnd > scanStart && hackEnd <= scanEnd)
                return true;

            return false;
        }

        /// <summary>
        ///     Apply the new bytes to address
        /// </summary>
        internal void Apply()
        {
            if (RelativeToPlayerBase)
            {
                if (!ObjectManager.Instance.IsIngame) return;
                if (ObjectManager.Instance.Player == null) return;
                _originalBytes ??= Memory.Reader.ReadBytes(Address, _customBytes.Length);
            }
            Memory.Reader.WriteBytes(Address, _customBytes);
        }

        /// <summary>
        ///     Restore the old bytes to the address
        /// </summary>
        internal void Remove()
        {
            if (RelativeToPlayerBase)
            {
                if (!ObjectManager.Instance.IsIngame) return;
                if (ObjectManager.Instance.Player == null) return;
            }
            if (DynamicHide && IsActivated)
                _customBytes = Memory.Reader.ReadBytes(Address, _originalBytes.Length);
            Memory.Reader.WriteBytes(Address, _originalBytes);
        }
    }
}
