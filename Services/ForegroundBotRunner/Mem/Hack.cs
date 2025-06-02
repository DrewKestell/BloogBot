namespace ForegroundBotRunner.Mem
{
    internal class Hack
    {
        internal Hack(string name, nint address, byte[] newBytes)
        {
            Name = name;
            Address = address;
            NewBytes = newBytes;

            OriginalBytes = MemoryManager.ReadBytes(address, newBytes.Length);
        }

        internal string Name { get; }

        internal nint Address { get; }

        internal byte[] NewBytes { get; }

        internal byte[] OriginalBytes { get; }

        internal bool IsWithinScanRange(nint scanStartAddress, int size)
        {
            var scanStart = (int)scanStartAddress;
            var scanEnd = (int)nint.Add(scanStartAddress, size);

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
