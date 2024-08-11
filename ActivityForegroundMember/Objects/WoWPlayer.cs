using BotRunner.Constants;
using BotRunner.Interfaces;

namespace ActivityForegroundMember.Objects
{
    public class WoWPlayer : WoWUnit, IWoWPlayer
    {
        internal WoWPlayer(
            nint pointer,
            ulong guid,
            WoWObjectType objectType)
            : base(pointer, guid, objectType)
        {
        }

        public bool IsEating => HasBuff("Food");

        public bool IsDrinking => HasBuff("Drink");

        public Race Race => throw new NotImplementedException();

        public Class Class => throw new NotImplementedException();
    }
}
