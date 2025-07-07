using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWItem(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.Item) : WoWObject(highGuid, objectType), IWoWItem
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

        public string Name { get; set; } = string.Empty;

        public HighGuid CreatedBy { get; set; } = new HighGuid(new byte[4], new byte[4]);

        public void Use()
        {

        }
        public void Loot()
        {

        }

        public override WoWObject Clone()
        {
            var clone = new WoWItem(HighGuid, ObjectType);
            clone.CopyFrom(this);
            return clone;
        }

        public override void CopyFrom(WoWObject sourceBase)
        {
            base.CopyFrom(sourceBase);

            if (sourceBase is not WoWItem source)
                return;

            ItemId = source.ItemId;
            Quantity = source.Quantity;
            StackCount = source.StackCount;
            MaxDurability = source.MaxDurability;
            RequiredLevel = source.RequiredLevel;
            Durability = source.Durability;
            Duration = source.Duration;
            PropertySeed = source.PropertySeed;
            RandomPropertiesId = source.RandomPropertiesId;
            ItemTextId = source.ItemTextId;
            IsCoins = source.IsCoins;
            ItemDynamicFlags = source.ItemDynamicFlags;
            Quality = source.Quality;

            Array.Copy(source.SpellCharges, SpellCharges, SpellCharges.Length);
            Array.Copy(source.Enchantments, Enchantments, Enchantments.Length);
        }
    }
}
