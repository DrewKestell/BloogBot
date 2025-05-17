using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWItem(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.Item) : WoWGameObject(highGuid, objectType), IWoWItem
    {
        public uint ItemId { get; set; }
        public uint Quantity { get; set; }
        public uint StackCount { get; set; }
        public uint MaxDurability { get; set; }
        public uint RequiredLevel { get; set; }
        public uint Durability { get; set; }
        public uint Duration { get; set; }
        public uint[] SpellCharges { get; } = new uint[5];
        public uint[] Enchantments { get; } = new uint[21];
        public uint PropertySeed { get; set; }
        public uint RandomPropertiesId { get; set; }
        public uint ItemTextId { get; set; }
        public bool IsCoins { get; set; }
        public ItemCacheInfo? Info { get; }
        public ItemDynFlags ItemDynamicFlags { get; set; }
        public ItemQuality Quality { get; set; }
        public HighGuid Owner { get; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid Contained { get; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid GiftCreator { get; } = new HighGuid(new byte[4], new byte[4]);

        public void Use()
        {

        }
        public void Loot()
        {

        }
        // Other properties and methods can be added here as needed
    }
}
