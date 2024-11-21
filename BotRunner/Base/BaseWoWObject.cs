using BotRunner.Interfaces;
using BotRunner.Models;
using PathfindingService.Models;

namespace BotRunner.Base
{
    public abstract class BaseWoWObject(nint pointer, HighGuid highGuid, WoWObjectType objectType = WoWObjectType.None) : IWoWObject
    {
        protected abstract nint GetDescriptorPtr();
        public nint Pointer { get; } = pointer;
        public HighGuid HighGuid { get; } = highGuid;
        public HighGuid CreatedBy { get; } = highGuid;
        public virtual ulong Guid => HighGuid.FullGuid;
        public virtual WoWObjectType ObjectType { get; set; } = objectType;
        public virtual uint LastUpated { get; set; }
        public virtual uint Entry { get; set; }
        public virtual float ScaleX { get; set; }
        public virtual float Height { get; set; }
        public virtual float Facing { get; set; }
        public virtual Position Position { get; set; } = new Position(0, 0, 0);
        public virtual bool InWorld { get; }
        public DynamicFlags DynamicFlags { get; set; }
        public virtual uint DisplayId { get; set; }
        public virtual uint Timestamp { get; set; }
        public bool InLosWith(Position position) => true;
        public bool InLosWith(IWoWObject objc) => InLosWith(objc.Position);
        public bool IsFacing(Position position) => Math.Abs(GetFacingForPosition(position) - Facing) < 0.05f;
        public bool IsFacing(IWoWObject objc) => IsFacing(objc.Position);
        public bool IsBehind(IWoWObject target)
        {
            if (target == null) return false;

            float facing = GetFacingForPosition(target.Position);

            var halfPi = Math.PI / 2;
            var twoPi = Math.PI * 2;
            var leftThreshold = target.Facing - halfPi;
            var rightThreshold = target.Facing + halfPi;

            bool condition;
            if (leftThreshold < 0)
                condition = facing < rightThreshold || facing > twoPi + leftThreshold;
            else if (rightThreshold > twoPi)
                condition = facing > leftThreshold || facing < rightThreshold - twoPi;
            else
                condition = facing > leftThreshold && facing < rightThreshold;

            return condition;
        }
        public Position GetPointBehindUnit(float distance)
        {
            var newX = Position.X + distance * (float)-Math.Cos(Facing);
            var newY = Position.Y + distance * (float)-Math.Sin(Facing);
            var end = new Position(newX, newY, Position.Z);
            return end;
        }
        public float GetFacingForPosition(Position position)
        {
            var f = (float)Math.Atan2(position.Y - Position.Y, position.X - Position.X);
            if (f < 0.0f)
                f += (float)Math.PI * 2.0f;
            else
            {
                if (f > (float)Math.PI * 2)
                    f -= (float)Math.PI * 2.0f;
            }
            return f;
        }
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
