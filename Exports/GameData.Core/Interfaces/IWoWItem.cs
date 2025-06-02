using GameData.Core.Enums;
using GameData.Core.Models;

namespace GameData.Core.Interfaces
{
    public interface IWoWItem : IWoWObject
    {
        string Name { get; }
        uint ItemId { get; }
        uint Quantity { get; }
        uint StackCount { get; }
        uint MaxDurability { get; }
        uint RequiredLevel { get; }
        uint Durability { get; }
        uint Duration { get; }
        uint[] SpellCharges { get; }
        uint[] Enchantments { get; }
        uint PropertySeed { get; }
        uint RandomPropertiesId { get; }
        uint ItemTextId { get; }
        bool IsCoins { get; }
        HighGuid Owner { get; }
        HighGuid Contained { get; }
        HighGuid CreatedBy { get; }
        HighGuid GiftCreator { get; }
        ItemCacheInfo? Info { get; }
        ItemDynFlags ItemDynamicFlags { get; set; }
        ItemQuality Quality { get; }
        uint DurabilityPercentage => (uint)((double)Durability / MaxDurability * 100);
        void Use();
        void Loot();
    }
}
