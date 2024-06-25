namespace MaNGOSDBDomain.Models
{
    public class GameObject
    {
        public int Guid { get; set; }
        public int Id { get; set; }
        public short Map { get; set; }
        public byte SpawnMask { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float Orientation { get; set; }
        public float Rotation0 { get; set; }
        public float Rotation1 { get; set; }
        public float Rotation2 { get; set; }
        public float Rotation3 { get; set; }
        public int SpawnTimeSecsMin { get; set; }
        public int SpawnTimeSecsMax { get; set; }
        public byte AnimProgress { get; set; }
        public byte State { get; set; }
    }
}
