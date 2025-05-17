namespace GameData.Core.Models
{
    public class UpdateMask
    {
        public uint FieldCount { get; private set; }
        public uint BlockCount { get; private set; }
        private byte[] bits;

        public UpdateMask()
        {
            FieldCount = 0;
            BlockCount = 0;
            bits = null;
        }

        public void SetBit(uint index)
        {
            bits[index / 8] |= (byte)(1 << (int)(index % 8));
        }

        public void UnsetBit(uint index)
        {
            bits[index / 8] &= (byte)~(1 << (int)(index % 8));
        }

        public bool GetBit(uint index)
        {
            return (bits[index / 8] & 1 << (int)(index % 8)) != 0;
        }

        public void SetCount(uint valuesCount)
        {
            FieldCount = valuesCount;
            BlockCount = (valuesCount + 31) / 32;
            bits = new byte[BlockCount * 4]; // 4 bytes per block
        }

        public void Clear()
        {
            if (bits != null)
            {
                Array.Clear(bits, 0, bits.Length);
            }
        }

        public void AppendToPacket(BinaryWriter writer)
        {
            for (uint i = 0; i < BlockCount; ++i)
            {
                uint maskPart = 0;
                for (uint j = 0; j < 32; ++j)
                {
                    if (GetBit(32 * i + j))
                    {
                        maskPart |= (uint)(1 << (int)j);
                    }
                }
                writer.Write(maskPart);
            }
        }

        public static UpdateMask ReadFromPacket(BinaryReader reader, uint fieldCount)
        {
            UpdateMask updateMask = new();
            updateMask.SetCount(fieldCount);

            for (uint i = 0; i < updateMask.BlockCount; ++i)
            {
                uint maskPart = reader.ReadUInt32();
                for (uint j = 0; j < 32; ++j)
                {
                    if ((maskPart & 1 << (int)j) != 0)
                    {
                        updateMask.SetBit(32 * i + j);
                    }
                }
            }

            return updateMask;
        }
    }

}
