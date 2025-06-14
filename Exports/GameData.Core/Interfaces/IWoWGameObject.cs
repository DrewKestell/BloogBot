using GameData.Core.Enums;
using GameData.Core.Models;
using Position = GameData.Core.Models.Position;

namespace GameData.Core.Interfaces
{
    public interface IWoWGameObject : IWoWObject
    {
        HighGuid CreatedBy { get; }
        uint DisplayId { get; }
        uint Flags { get; }
        float[] Rotation { get; }
        GOState GoState { get; }
        DynamicFlags DynamicFlags { get; }
        uint FactionTemplate { get; }
        uint TypeId { get; }
        uint Level { get; }
        uint ArtKit { get; }
        uint AnimProgress { get; }
        bool CanBeLooted => DynamicFlags.HasFlag(DynamicFlags.CanBeLooted);
        bool TappedByOther => DynamicFlags.HasFlag(DynamicFlags.Tapped) && !DynamicFlags.HasFlag(DynamicFlags.TappedByMe);
        Position GetPointBehindUnit(float distance);
        void Interact();
        public bool IsFacing(Position position) => Math.Abs(GetFacingForPosition(position) - Facing) < 0.05f;

        public bool IsFacing(IWoWObject obj) => obj != null && IsFacing(obj.Position);

        public bool InLosWith(Position position)
        {
            if (position == null)
                return false;

            // Simple 2D LOS check ignoring elevation and obstructions
            float dx = Position.X - position.X;
            float dy = Position.Y - position.Y;
            float distanceSq = dx * dx + dy * dy;

            // Tune max LOS range as needed; here assuming 60 yd = 60f
            return distanceSq <= 60f * 60f;
        }

        public bool InLosWith(IWoWObject obj) => obj != null && InLosWith(obj.Position);

        public bool IsBehind(IWoWGameObject target)
        {
            var halfPi = Math.PI / 2;
            var twoPi = Math.PI * 2;
            var leftThreshold = target.Facing - halfPi;
            var rightThreshold = target.Facing + halfPi;

            bool condition;
            if (leftThreshold < 0)
                condition = Facing < rightThreshold || Facing > twoPi + leftThreshold;
            else if (rightThreshold > twoPi)
                condition = Facing > leftThreshold || Facing < rightThreshold - twoPi;
            else
                condition = Facing > leftThreshold && Facing < rightThreshold;

            return condition && IsFacing(target.Position);
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
        private static float NormalizeAngle(float angle)
        {
            while (angle < 0) angle += (float)(2 * Math.PI);
            while (angle >= (float)(2 * Math.PI)) angle -= (float)(2 * Math.PI);
            return angle;
        }
        string Name { get; }
    }
}
