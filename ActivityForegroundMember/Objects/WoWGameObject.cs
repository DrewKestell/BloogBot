using BotRunner.Base;
using BotRunner.Interfaces;
using BotRunner.Models;

namespace ActivityForegroundMember.Objects
{
    public class WoWGameObject : WoWObject, IWoWGameObject
    {
        internal WoWGameObject(
            nint pointer,
            HighGuid guid,
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
    }
}
