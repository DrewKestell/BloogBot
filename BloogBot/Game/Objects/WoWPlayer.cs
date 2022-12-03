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
                if (ClientHelper.ClientVersion == ClientVersion.WotLK)
                {
                    return MemoryManager.ReadInt(Pointer + 0xC70) > 0;
                }
                else
                {
                    return HasBuff("Food");
                }
            }
        }

        public bool IsDrinking
        {
            get
            {
                if (ClientHelper.ClientVersion == ClientVersion.WotLK)
                {
                    return MemoryManager.ReadInt(Pointer + 0xF3C) == 4;
                }
                else
                {
                    return HasBuff("Drink"); ;
                }
            }
        }
    }
}
