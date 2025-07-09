using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWContainer(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.Container) : WoWItem(highGuid, objectType), IWoWContainer
    {
        public int NumOfSlots { get; set; }

        public uint[] Slots { get; } = new uint[32];

        public ulong GetItemGuid(int parSlot)
        {
            throw new NotImplementedException();
        }

        public override WoWObject Clone()
        {
            var clone = new WoWContainer(HighGuid, ObjectType);
            clone.CopyFrom(this);
            return clone;
        }

        public override void CopyFrom(WoWObject sourceBase)
        {
            base.CopyFrom(sourceBase);

            if (sourceBase is not WoWContainer source)
                return;

            NumOfSlots = source.NumOfSlots;
            Array.Copy(source.Slots, Slots, Slots.Length);
        }
    }
}
