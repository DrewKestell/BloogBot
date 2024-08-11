using BotRunner.Interfaces;
using PathfindingService.Models;

namespace ActivityForegroundMember.Objects
{
    public class WoWGameObject : WoWObject, IWoWGameObject
    {
        internal WoWGameObject(
            nint pointer,
            ulong guid,
            WoWObjectType objectType)
            : base(pointer, guid, objectType)
        {
        }

        public uint DisplayId => throw new NotImplementedException();

        public GOState GoState => throw new NotImplementedException();

        public uint ArtKit => throw new NotImplementedException();

        public uint AnimProgress => throw new NotImplementedException();

        public uint Level => throw new NotImplementedException();

        public uint FactionTemplate => throw new NotImplementedException();

        public uint TypeId => throw new NotImplementedException();

        public Position GetPointBehindObject(float distanceToMove)
        {
            throw new NotImplementedException();
        }

        public void SetDisplayId(uint displayId)
        {
            throw new NotImplementedException();
        }

        public void SetGoState(GOState state)
        {
            throw new NotImplementedException();
        }
    }
}
