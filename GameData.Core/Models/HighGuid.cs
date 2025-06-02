namespace GameData.Core.Models
{
    public class HighGuid
    {
        public byte[] LowGuidValue { get; set; } = new byte[4];
        public byte[] HighGuidValue { get; set; } = new byte[4];

        public ulong FullGuid
        {
            get
            {
                ulong low = BitConverter.ToUInt32(LowGuidValue, 0);
                ulong high = BitConverter.ToUInt32(HighGuidValue, 0);
                return (high << 32) | low;
            }
        }

        // Constructor using byte arrays
        public HighGuid(byte[] lowGuidValue, byte[] highGuidValue)
        {
            if (lowGuidValue.Length != 4 || highGuidValue.Length != 4)
                throw new ArgumentException("Low and High GUID byte arrays must be 4 bytes each.");

            LowGuidValue = lowGuidValue;
            HighGuidValue = highGuidValue;
        }

        // Constructor using a ulong
        public HighGuid(ulong guid)
        {
            LowGuidValue = BitConverter.GetBytes((uint)(guid & 0xFFFFFFFF));
            HighGuidValue = BitConverter.GetBytes((uint)(guid >> 32));
        }
    }
}
