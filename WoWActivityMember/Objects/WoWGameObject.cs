using static WoWActivityMember.Constants.Enums;

namespace WoWActivityMember.Objects
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
