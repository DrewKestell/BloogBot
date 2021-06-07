using BloogBot.Game.Enums;
using System;

namespace BloogBot.Game.Objects
{
    public class WoWGameObject : WoWObject
    {
        internal WoWGameObject(
            IntPtr pointer,
            ulong guid,
            ObjectType objectType)
            : base(pointer, guid, objectType)
        {
        }
    }
}
