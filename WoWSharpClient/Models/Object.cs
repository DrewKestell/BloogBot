using BotRunner.Interfaces;
using PathfindingService.Models;

namespace WoWSharpClient.Models
{
    public class Object(byte[] lowGuid, byte[] highGuid, WoWObjectType objectType = WoWObjectType.None) : IWoWObject
    {
        private readonly WoWObjectType _objectType = objectType;
        private readonly byte[] _lowGuid = lowGuid;
        private readonly byte[] _highGuid = highGuid;

        public ulong Guid => BitConverter.ToUInt64([.. _lowGuid, .. _highGuid], 0);
        public WoWObjectType ObjectType => _objectType;
        public uint LastUpated { get; set; }
        public uint Entry { get; internal set; }
        public float ScaleX { get; set; } = 1.0f;
        public float Height { get; set; } = 1.0f;
        public float Facing { get; set; }
        public Position Position { get; set; } = new(0, 0, 0);
        public uint Padding { get; set; }
        public bool InWorld { get; private set; } = false;

        public nint Pointer => 0;

        public Position GetPointBehindUnit(float distance)
        {
            throw new NotImplementedException();
        }

        public bool InLosWith(Position position)
        {
            throw new NotImplementedException();
        }

        public bool InLosWith(IWoWObject objc)
        {
            throw new NotImplementedException();
        }

        public void Interact()
        {
            throw new NotImplementedException();
        }

        public bool IsBehind(IWoWUnit unit)
        {
            throw new NotImplementedException();
        }

        public bool IsFacing(Position position)
        {
            throw new NotImplementedException();
        }

        public bool IsFacing(IWoWObject objc)
        {
            throw new NotImplementedException();
        }
    }
}
