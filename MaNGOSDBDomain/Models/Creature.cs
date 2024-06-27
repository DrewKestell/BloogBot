using System.Numerics;

namespace MaNGOSDBDomain.Models
{
    public class Creature
    {
        public int Guid { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public short Map { get; set; }
        public byte SpawnMask { get; set; }
        public int ModelId { get; set; }
        public int EquipmentId { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public Vector3 SpawnPosition { get; set; }
        public float Orientation { get; set; }
        public int SpawnTimeSecsMin { get; set; }
        public int SpawnTimeSecsMax { get; set; }
        public float SpawnDist { get; set; }
        public int CurrentWaypoint { get; set; }
        public int CurHealth { get; set; }
        public int CurMana { get; set; }
        public byte DeathState { get; set; }
        public byte MovementType { get; set; }
    }
}
