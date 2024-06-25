using static WoWClientBot.Constants.Enums;

namespace WoWClientBot.Objects
{
    public class WoWPlayer : WoWUnit
    {
        internal WoWPlayer(
            IntPtr pointer,
            ulong guid,
            WoWObjectTypes objectType)
            : base(pointer, guid, objectType)
        {
        }

        public bool IsEating => HasBuff("Food");

        public bool IsDrinking => HasBuff("Drink");
    }
}
