namespace MaNGOSDBDomain.Models
{
    public class CreatureMovementTemplate
    {
        public int Entry { get; set; }
        public int PathId { get; set; }
        public int Point { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float Orientation { get; set; }
        public int WaitTime { get; set; }
        public int ScriptId { get; set; }
        public string Comment { get; set; }
    }
}
