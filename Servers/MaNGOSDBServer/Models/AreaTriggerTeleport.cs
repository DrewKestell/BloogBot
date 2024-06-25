namespace MaNGOSDBDomain.Models
{
    public class AreaTriggerTeleport
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte RequiredLevel { get; set; }
        public int RequiredItem { get; set; }
        public int RequiredItem2 { get; set; }
        public int RequiredQuestDone { get; set; }
        public ushort TargetMap { get; set; }
        public float TargetPositionX { get; set; }
        public float TargetPositionY { get; set; }
        public float TargetPositionZ { get; set; }
        public float TargetOrientation { get; set; }
        public string StatusFailedText { get; set; }
        public int ConditionId { get; set; }
    }
}
