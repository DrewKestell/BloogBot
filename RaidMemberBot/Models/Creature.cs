using RaidMemberBot.Objects;

namespace RaidMemberBot.Models
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
        public float LocationX { get; set; }
        public float LocationY { get; set; }
        public float LocationZ { get; set; }
        public Location SpawnLocation { get; set; }
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
