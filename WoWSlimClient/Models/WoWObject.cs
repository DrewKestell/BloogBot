
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using WoWSlimClient.Models;
using static WoWSlimClient.Models.Enums;

namespace WoWSlimClient.Models
{
    public abstract class WoWObject
    {
        public virtual ulong Guid { get; set; }
        public virtual WoWObjectTypes ObjectType { get; set; }
        public float ScaleX { get; set; }
        public float Facing { get; set; }
        public Position Position { get; set; }
        public string Name { get; set; }

        public Position GetPointBehindUnit(float parDistanceToMove)
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
