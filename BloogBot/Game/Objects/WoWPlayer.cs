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

        public bool IsEating
        {
            get
            {
                return HasBuff("Food");
            }
        }

        public bool IsDrinking
        {
            get
            {
                return HasBuff("Drink"); ;
            }
        }
    }
}
