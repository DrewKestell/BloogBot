namespace WoWSlimClient.Models
{
    public class WoWObject(byte[] lowGuid, byte[] highGuid, WoWObjectType objectType = WoWObjectType.None)
    {
        private readonly WoWObjectType _objectType = objectType;
        private readonly byte[] _lowGuid = lowGuid;
        private readonly byte[] _highGuid = highGuid;
        public uint LastUpated { get; set; }
        public ulong Guid => BitConverter.ToUInt64(_lowGuid.Concat(_highGuid).ToArray(), 0);
        public WoWObjectType ObjectType => _objectType;
        public uint Padding { get; set; }
        public float ScaleX { get; set; }
        public float Facing { get; set; }
        public Position Position { get; set; } = new Position(0, 0, 0);
        public uint Entry { get; internal set; }
    }
}
