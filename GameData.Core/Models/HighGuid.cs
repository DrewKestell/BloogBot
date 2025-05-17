namespace GameData.Core.Models
{
    public class HighGuid(byte[] lowGuidValue, byte[] highGuidValue)
    {
        public byte[] LowGuidValue { get; set; } = lowGuidValue;
        public byte[] HighGuidValue { get; set; } = highGuidValue;
        public ulong FullGuid
        {
            get
            {
                // Ensure high and low are 4 bytes each
                ulong low = BitConverter.ToUInt32(LowGuidValue, 0);
                ulong high = BitConverter.ToUInt32(HighGuidValue, 0);
                return (high << 32) | low;
            }
        }
    }
}
