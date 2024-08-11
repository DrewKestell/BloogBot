using BotRunner.Interfaces;
using PathfindingService.Models;

namespace WoWSharpClient.Models
{
    public class GameObject(byte[] lowGuid, byte[] highGuid, WoWObjectType objectType = WoWObjectType.GameObj) : Object(lowGuid, highGuid, objectType), IWoWGameObject
    {
        public string Name { get; set; }
        public uint DisplayId { get; set; } = 0;
        public GOState GoState { get; set; } = GOState.Active;
        public uint ArtKit { get; set; } = 0;
        public uint AnimProgress { get; set; } = 0;
        public uint Level { get; set; } = 1;
        public uint FactionTemplate { get; set; } = 0;
        public uint TypeId { get; set; } = 0;

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

        Position IWoWGameObject.GetPointBehindObject(float distanceToMove)
        {
            throw new NotImplementedException();
        }
    }
}
