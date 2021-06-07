using BloogBot.Game.Enums;
using System;

namespace BloogBot.Game.Objects
{
    public class WoWPlayer : WoWUnit
    {
        internal WoWPlayer(
            IntPtr pointer,
            ulong guid,
            ObjectType objectType)
            : base(pointer, guid, objectType)
        {
        }

        public bool IsEating => HasBuff("Food");

        public bool IsDrinking => HasBuff("Drink");
    }
}
