using BotRunner.Base;
using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Models;

namespace WoWSharpClient.Models
{
    public class Item(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.Item) : Object(highGuid, objectType), IWoWItem
    {
        public uint ItemId { get; set; }
        public HighGuid Owner { get; set; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid Contained { get; internal set; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid GiftCreator { get; internal set; } = new HighGuid(new byte[4], new byte[4]);
        public uint StackCount { get; set; }
        public uint Duration { get; set; }
        public ItemDynFlags Flags { get; set; }
        public uint[] SpellCharges { get; } = new uint[5];
        public uint[] Enchantments { get; } = new uint[21];
        public uint PropertySeed { get; set; }
        public uint RandomPropertiesId { get; set; }
        public uint ItemTextId { get; set; }
        public uint Durability { get; set; }
        public uint MaxDurability { get; set; }
        public uint RequiredLevel { get; set; }
        public ItemQuality Quality { get; set; } = ItemQuality.Poor;
        public uint DurabilityPercentage => (uint)((double)Durability / MaxDurability * 100);
        public bool IsCoins => false;
        public IWoWItemCacheInfo Info { get; set; }

        public void Use()
        {

        }

        public void Loot()
        {

        }
    }
}
