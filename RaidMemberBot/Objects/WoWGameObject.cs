using System;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Objects
{
    public class WoWGameObject : WoWObject
    {
        internal WoWGameObject(
            IntPtr pointer,
            ulong guid,
            WoWObjectTypes objectType)
            : base(pointer, guid, objectType)
        {
        }
    }
}
