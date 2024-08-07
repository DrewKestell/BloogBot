using BotRunner.Constants;
using BotRunner.Interfaces;

namespace WoWSharpClient.Models
{
    public class Item(byte[] lowGuid, byte[] highGuid, WoWObjectType objectType = WoWObjectType.Item) : GameObject(lowGuid, highGuid, objectType), IWoWItem
    {
        public uint ItemId { get; set; }

        public uint StackCount { get; set; }
        public uint MaxDurability { get; set; }
        public uint RequiredLevel { get; set; }

        public void Use()
        {

        }

        public void Loot()
        {

        }

        public ItemQuality Quality { get; set; } = ItemQuality.Poor;

        public uint Durability  { get; set; }

        public uint DurabilityPercentage => (uint)((double)Durability / MaxDurability * 100);

        public bool IsCoins => false;

        public IWoWItemCacheInfo Info => throw new NotImplementedException();
    }
}
