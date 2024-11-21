using BotRunner.Base;
using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Models;

namespace ActivityForegroundMember.Objects
{
    public class WoWPlayer : WoWUnit, IWoWPlayer
    {
        internal WoWPlayer(
            nint pointer,
            HighGuid guid,
            WoWObjectType objectType)
            : base(pointer, guid, objectType)
        {
        }

        public bool IsEating => HasBuff("Food");

        public bool IsDrinking => HasBuff("Drink");

        public Race Race => throw new NotImplementedException();

        public Class Class => throw new NotImplementedException();

        public void OfferTrade()
        {
            throw new NotImplementedException();
        }
    }
}
