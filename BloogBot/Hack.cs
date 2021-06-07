using System;

namespace BloogBot
{
    class Hack
    {
        internal Hack(string name, IntPtr address, byte[] newBytes)
        {
            Name = name;
            Address = address;
            NewBytes = newBytes;

            OriginalBytes = MemoryManager.ReadBytes(address, newBytes.Length);
        }

        internal string Name { get; }

        internal IntPtr Address { get; }

        internal byte[] NewBytes { get; }

        internal byte[] OriginalBytes { get; }

        internal bool IsWithinScanRange(IntPtr scanStartAddress, int size)
        {
            var scanStart = (int)scanStartAddress;
            var scanEnd = (int)IntPtr.Add(scanStartAddress, size);

            var hackStart = (int)Address;
            var hackEnd = (int)Address + NewBytes.Length;

            if (hackStart >= scanStart && hackStart < scanEnd)
                return true;

            if (hackEnd > scanStart && hackEnd <= scanEnd)
                return true;

            return false;
        }
    }
}
