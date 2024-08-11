using BotRunner.Constants;

namespace BotRunner.Interfaces
{
    public interface IWoWItemCacheInfo
    {
        private static readonly IList<ItemSubclass[]> ItemSubclasses =
        [
            // these are untested for Vanilla
            [
                ItemSubclass.Consumable,
                ItemSubclass.Potion,
                ItemSubclass.Elixir,
                ItemSubclass.Flask,
                ItemSubclass.Scroll,
                ItemSubclass.FoodAndDrink,
                ItemSubclass.ItemEnhancement,
                ItemSubclass.Bandage,
                ItemSubclass.Other
            ],
            [
                ItemSubclass.Container,
                ItemSubclass.SoulBag,
                ItemSubclass.HerbBag,
                ItemSubclass.EnchantingBag,
                ItemSubclass.EngineeringBag,
                ItemSubclass.GemBag,
                ItemSubclass.MiningBag,
                ItemSubclass.LeatherworkingBag,
                ItemSubclass.InscriptionBag
            ],
            [
                ItemSubclass.OneHandedAxe,
                ItemSubclass.TwoHandedAxe,
                ItemSubclass.Bow,
                ItemSubclass.Gun,
                ItemSubclass.OneHandedMace,
                ItemSubclass.TwoHandedMace,
                ItemSubclass.Polearm,
                ItemSubclass.OneHandedSword,
                ItemSubclass.TwoHandedSword,
                ItemSubclass.Obsolete,
                ItemSubclass.Staff,
                ItemSubclass.OneHandedExotic,
                ItemSubclass.TwoHandedExotic,
                ItemSubclass.FistWeapon,
                ItemSubclass.MiscellaneousWeapon,
                ItemSubclass.Dagger,
                ItemSubclass.Thrown,
                ItemSubclass.Spear,
                ItemSubclass.Crossbow,
                ItemSubclass.Wand,
                ItemSubclass.FishingPole
            ],
            [
                ItemSubclass.RedJewel,
                ItemSubclass.BlueJewel,
                ItemSubclass.YellowJewel,
                ItemSubclass.PurpleJewel,
                ItemSubclass.GreenJewel,
                ItemSubclass.OrangeJewel,
                ItemSubclass.MetaJewel,
                ItemSubclass.SimpleJewel,
                ItemSubclass.PrismaticJewel
            ],
            [
                ItemSubclass.MiscellaneousArmor,
                ItemSubclass.Cloth,
                ItemSubclass.Leather,
                ItemSubclass.Mail,
                ItemSubclass.Plate,
                ItemSubclass.BucklerOBSOLETE,
                ItemSubclass.Shield,
                ItemSubclass.Libram,
                ItemSubclass.Idol,
                ItemSubclass.Totem,
                ItemSubclass.Sigil
            ],
            [
                ItemSubclass.Reagent
            ],
            [
                ItemSubclass.WandOBSOLETE,
                ItemSubclass.BoltOBSOLETE,
                ItemSubclass.Arrow,
                ItemSubclass.Bullet,
                ItemSubclass.ThrownOBSOLETE
            ],
            [
                ItemSubclass.TradeGood,
                ItemSubclass.Parts,
                ItemSubclass.Explosives,
                ItemSubclass.Devices,
                ItemSubclass.CraftingJewelcrafting,
                ItemSubclass.CraftingCloth,
                ItemSubclass.CraftingLeather,
                ItemSubclass.CraftingMetalAndStone,
                ItemSubclass.CraftingMeat,
                ItemSubclass.CraftingHerb,
                ItemSubclass.CraftingElemental,
                ItemSubclass.CraftingOther,
                ItemSubclass.CraftingEnchanting,
                ItemSubclass.CraftingMaterials,
                ItemSubclass.CraftingArmorEnchantment,
                ItemSubclass.CraftingWeaponEnchantment
            ],
            [
                ItemSubclass.GenericOBSOLETE
            ],
            [
                ItemSubclass.RecipeBook,
                ItemSubclass.RecipeLeatherworking,
                ItemSubclass.RecipeTailoring,
                ItemSubclass.RecipeEngineering,
                ItemSubclass.RecipeBlacksmithing,
                ItemSubclass.RecipeCooking,
                ItemSubclass.RecipeAlchemy,
                ItemSubclass.RecipeFirstAid,
                ItemSubclass.RecipeEnchanting,
                ItemSubclass.RecipeFishing,
                ItemSubclass.RecipeJewelcrafting
            ],
            [
                ItemSubclass.MoneyOBSOLETE
            ],
            [
                ItemSubclass.Quiver1OBSOLETE,
                ItemSubclass.Quiver2OBSOLETE,
                ItemSubclass.Quiver,
                ItemSubclass.AmmoPouch
            ],
            [
                ItemSubclass.Quest
            ],
            [
                ItemSubclass.Key,
                ItemSubclass.Lockpick
            ],
            [
                ItemSubclass.Permanent
            ],
            [
                ItemSubclass.MiscJunk,
                ItemSubclass.MiscReagent,
                ItemSubclass.MiscPet,
                ItemSubclass.MiscHoliday,
                ItemSubclass.MiscOther,
                ItemSubclass.MiscMount
            ]
        ];
        ItemCacheEntry Info { get; }

        ItemClass ItemClass => Info.ItemClass;

        ItemSubclass ItemSubclass => ItemSubclasses.ElementAt((int)ItemClass)[Info.ItemSubclassID];

        ItemQuality Quality => Info.ItemQuality;

        EquipSlot EquipSlot => Info.EquipSlot;

        int RequiredLevel => Info.RequiredLevel;

        int MaxDurability => Info.MaxDurability;

        string Name => Info.NamePtr.ToString();
    }

    public class ItemCacheEntry(nint baseAddress)
    {
        private readonly nint baseAddress = baseAddress;
        internal ItemClass ItemClass;

        internal int ItemSubclassID;

        internal ItemQuality ItemQuality;

        internal EquipSlot EquipSlot;

        internal int RequiredLevel;

        internal int MaxDurability;

        internal nint NamePtr;
    }
}
