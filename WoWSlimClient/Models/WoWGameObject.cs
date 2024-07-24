

namespace WoWSlimClient.Models
{
    public class WoWGameObject(byte[] lowGuid, byte[] highGuid, WoWObjectType objectType = WoWObjectType.GameObj) : WoWObject(lowGuid, highGuid, objectType)
    {
        public string Name { get; set; }
        public uint DisplayId { get; set; }
        public DynamicFlags DynamicFlags { get; set; }
        public uint FactionTemplate { get; set; }
        public uint TypeId { get; set; }
        public uint Level { get; set; }
        public uint ArtKit { get; set; }
        public uint AnimProgress { get; set; }
        public Position GetPointBehindObject(float parDistanceToMove)
        {
            var newX = Position.X + parDistanceToMove * (float)-Math.Cos(Facing);
            var newY = Position.Y + parDistanceToMove * (float)-Math.Sin(Facing);
            var end = new Position(newX, newY, Position.Z);
            return end;
        }
        public void Interact()
        {

        }
    }
}
