namespace WoWSlimClient.Models
{
    public class WoWObject(byte[] lowGuid, byte[] highGuid, WoWObjectType objectType = WoWObjectType.None)
    {
        private readonly WoWObjectType _objectType = objectType;
        private readonly byte[] _lowGuid = lowGuid;
        private readonly byte[] _highGuid = highGuid;

        public ulong Guid => BitConverter.ToUInt64(_lowGuid.Concat(_highGuid).ToArray(), 0);
        public WoWObjectType ObjectType => _objectType;
        public uint LastUpated { get; set; }
        public uint Entry { get; internal set; }
        public float ScaleX { get; set; } = 1.0f;
        public float Facing { get; set; }
        public Position Position { get; set; } = new Position(0, 0, 0);
        public uint Padding { get; set; }
        public bool InWorld { get; private set; } = false;

    }
    [Flags]
    public enum HighGuid
    {
        HIGHGUID_ITEM = 0x4000,                       // blizz 4000
        HIGHGUID_CONTAINER = 0x4000,                       // blizz 4000
        HIGHGUID_PLAYER = 0x0000,                       // blizz 0000
        HIGHGUID_GAMEOBJECT = 0xF110,                       // blizz F110
        HIGHGUID_TRANSPORT = 0xF120,                       // blizz F120 (for GAMEOBJECT_TYPE_TRANSPORT)
        HIGHGUID_UNIT = 0xF130,                       // blizz F130
        HIGHGUID_PET = 0xF140,                       // blizz F140
        HIGHGUID_DYNAMICOBJECT = 0xF100,                       // blizz F100
        HIGHGUID_CORPSE = 0xF101,                       // blizz F100
        HIGHGUID_MO_TRANSPORT = 0x1FC0,                       // blizz 1FC0 (for GAMEOBJECT_TYPE_MO_TRANSPORT)
    }
    public enum WoWObjectType
    {
        None,
        Item,
        Container,
        Unit,
        Player,
        GameObj,
        DynamicObj,
        Corpse
    }
}
