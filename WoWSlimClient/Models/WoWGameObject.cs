namespace WoWSlimClient.Models
{
    public class WoWGameObject : WoWObject
    {
        public WoWGameObject(byte[] lowGuid, byte[] highGuid, WoWObjectType objectType = WoWObjectType.GameObj)
            : base(lowGuid, highGuid, objectType)
        {
            DisplayId = 0;
            GoState = GOState.Active;
            ArtKit = 0;
            AnimProgress = 0;
            Level = 1;
            FactionTemplate = 0;
            TypeId = 0;
        }

        public string Name { get; set; }
        public uint DisplayId { get; set; }
        public GOState GoState { get; set; }
        public uint ArtKit { get; set; }
        public uint AnimProgress { get; set; }
        public uint Level { get; set; }
        public uint FactionTemplate { get; set; }
        public uint TypeId { get; set; }

        public Position GetPointBehindObject(float distanceToMove)
        {
            var newX = Position.X + distanceToMove * (float)-Math.Cos(Facing);
            var newY = Position.Y + distanceToMove * (float)-Math.Sin(Facing);
            return new Position(newX, newY, Position.Z);
        }

        public void Interact()
        {
            // Implementation for interaction
        }

        public void SetDisplayId(uint displayId)
        {
            DisplayId = displayId;
        }

        public void SetGoState(GOState state)
        {
            GoState = state;
        }
    }

    public enum GOState
    {
        Active,
        Ready,
        ActiveAlternative
    }
}
