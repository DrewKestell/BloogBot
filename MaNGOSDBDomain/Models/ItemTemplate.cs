namespace MaNGOSDBDomain.Models
{
    public class ItemTemplate
    {
        private static readonly float ItemLevelWeight = 1.0f;

        private static readonly float ArmorWeight = 1.0f;
        private static readonly float BlockWeight = 1.0f;

        private static readonly float RangeModWeight = 1.0f;

        private static readonly float DmgMin1Weight = 1.0f;
        private static readonly float DmgMin2Weight = 1.0f;
        private static readonly float DmgMin3Weight = 1.0f;
        private static readonly float DmgMin4Weight = 1.0f;
        private static readonly float DmgMin5Weight = 1.0f;
        private static readonly float DmgMax1Weight = 1.0f;
        private static readonly float DmgMax2Weight = 1.0f;
        private static readonly float DmgMax3Weight = 1.0f;
        private static readonly float DmgMax4Weight = 1.0f;
        private static readonly float DmgMax5Weight = 1.0f;

        private static readonly float AgilityWeight = 1.0f;
        private static readonly float StrengthWeight = 1.0f;
        private static readonly float IntellectWeight = 1.0f;
        private static readonly float SpiritWeight = 1.0f;
        private static readonly float StaminaWeight = 1.0f;
        public int Entry { get; set; }
        public short Patch { get; set; }
        public short Class { get; set; }
        public short Subclass { get; set; }
        public string Name { get; set; } = "None";
        public int DisplayId { get; set; }
        public short Quality { get; set; }
        public int Flags { get; set; }
        public short BuyCount { get; set; }
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public short InventoryType { get; set; }
        public int AllowableClass { get; set; }
        public int AllowableRace { get; set; }
        public short ItemLevel { get; set; }
        public short RequiredLevel { get; set; }
        public short RequiredSkill { get; set; }
        public short RequiredSkillRank { get; set; }
        public int RequiredSpell { get; set; }
        public int RequiredHonorRank { get; set; }
        public int RequiredCityRank { get; set; }
        public short RequiredReputationFaction { get; set; }
        public short RequiredReputationRank { get; set; }
        public short MaxCount { get; set; }
        public short Stackable { get; set; }
        public short ContainerSlots { get; set; }
        public short StatType1 { get; set; }
        public short StatValue1 { get; set; }
        public short StatType2 { get; set; }
        public short StatValue2 { get; set; }
        public short StatType3 { get; set; }
        public short StatValue3 { get; set; }
        public short StatType4 { get; set; }
        public short StatValue4 { get; set; }
        public short StatType5 { get; set; }
        public short StatValue5 { get; set; }
        public short StatType6 { get; set; }
        public short StatValue6 { get; set; }
        public short StatType7 { get; set; }
        public short StatValue7 { get; set; }
        public short StatType8 { get; set; }
        public short StatValue8 { get; set; }
        public short StatType9 { get; set; }
        public short StatValue9 { get; set; }
        public short StatType10 { get; set; }
        public short StatValue10 { get; set; }
        public float DmgMin1 { get; set; }
        public float DmgMax1 { get; set; }
        public short DmgType1 { get; set; }
        public float DmgMin2 { get; set; }
        public float DmgMax2 { get; set; }
        public short DmgType2 { get; set; }
        public float DmgMin3 { get; set; }
        public float DmgMax3 { get; set; }
        public short DmgType3 { get; set; }
        public float DmgMin4 { get; set; }
        public float DmgMax4 { get; set; }
        public short DmgType4 { get; set; }
        public float DmgMin5 { get; set; }
        public float DmgMax5 { get; set; }
        public short DmgType5 { get; set; }
        public short Armor { get; set; }
        public short HolyResistance { get; set; }
        public short FireResistance { get; set; }
        public short NatureResistance { get; set; }
        public short FrostResistance { get; set; }
        public short ShadowResistance { get; set; }
        public short ArcaneResistance { get; set; }
        public short Delay { get; set; }
        public short AmmoType { get; set; }
        public float RangedModRange { get; set; }
        public int SpellId1 { get; set; }
        public short SpellTrigger1 { get; set; }
        public short SpellCharges1 { get; set; }
        public float SpellPpmRate1 { get; set; }
        public int SpellCooldown1 { get; set; }
        public short SpellCategory1 { get; set; }
        public int SpellCategoryCooldown1 { get; set; }
        public int SpellId2 { get; set; }
        public short SpellTrigger2 { get; set; }
        public short SpellCharges2 { get; set; }
        public float SpellPpmRate2 { get; set; }
        public int SpellCooldown2 { get; set; }
        public short SpellCategory2 { get; set; }
        public int SpellCategoryCooldown2 { get; set; }
        public int SpellId3 { get; set; }
        public short SpellTrigger3 { get; set; }
        public short SpellCharges3 { get; set; }
        public float SpellPpmRate3 { get; set; }
        public int SpellCooldown3 { get; set; }
        public short SpellCategory3 { get; set; }
        public int SpellCategoryCooldown3 { get; set; }
        public int SpellId4 { get; set; }
        public short SpellTrigger4 { get; set; }
        public short SpellCharges4 { get; set; }
        public float SpellPpmRate4 { get; set; }
        public int SpellCooldown4 { get; set; }
        public short SpellCategory4 { get; set; }
        public int SpellCategoryCooldown4 { get; set; }
        public int SpellId5 { get; set; }
        public short SpellTrigger5 { get; set; }
        public short SpellCharges5 { get; set; }
        public float SpellPpmRate5 { get; set; }
        public int SpellCooldown5 { get; set; }
        public short SpellCategory5 { get; set; }
        public int SpellCategoryCooldown5 { get; set; }
        public short Bonding { get; set; }
        public string Description { get; set; }
        public int PageText { get; set; }
        public short LanguageID { get; set; }
        public short PageMaterial { get; set; }
        public int StartQuest { get; set; }
        public int LockId { get; set; }
        public short Material { get; set; }
        public short Sheath { get; set; }
        public int RandomProperty { get; set; }
        public int Block { get; set; }
        public int ItemSet { get; set; }
        public short MaxDurability { get; set; }
        public int Area { get; set; }
        public short Map { get; set; }
        public int BagFamily { get; set; }
        public string ScriptName { get; set; }
        public int DisenchantID { get; set; }
        public short FoodType { get; set; }
        public int MinMoneyLoot { get; set; }
        public int MaxMoneyLoot { get; set; }
        public int Duration { get; set; }
        public short ExtraFlags { get; set; }
        public int OtherTeamEntry { get; set; }
        public string DisplayName => $"{Name} Lvl{RequiredLevel} iLvl{ItemLevel}";

        public bool CanEquip
        {
            get
            {
                switch (ItemClass)
                {
                    case ItemClass.AxeOneHand:
                        return false;
                    case ItemClass.AxeTwoHand:
                        return false;
                    case ItemClass.Bow:
                        return false;
                    case ItemClass.Gun:
                        return false;
                    case ItemClass.MaceOneHand:
                        return false;
                    case ItemClass.MaceTwoHand:
                        return false;
                    case ItemClass.Polearm:
                        return false;
                    case ItemClass.SwordOneHand:
                        return false;
                    case ItemClass.SwordTwoHand:
                        return false;
                    case ItemClass.Staff:
                        return false;
                    case ItemClass.Fist:
                        return false;
                    case ItemClass.Dagger:
                        return false;
                    case ItemClass.Thrown:
                        return false;
                    case ItemClass.Crossbow:
                        return false;
                    case ItemClass.Wand:
                        return false;
                    case ItemClass.Cloth:
                        return false;
                    case ItemClass.Leather:
                        return false;
                    case ItemClass.Mail:
                        return false;
                    case ItemClass.Plate:
                        return false;
                    case ItemClass.Shield:
                        return false;
                }

                return true;
            }
        }
        public ItemClass ItemClass
        {
            get
            {
                switch (Class)
                {
                    case 0:
                        switch (Subclass)
                        {
                            case 0:
                                return ItemClass.Consumable;
                        }
                        break;
                    case 1:
                        switch (Subclass)
                        {
                            case 0:
                                return ItemClass.Bag;
                            case 1:
                                return ItemClass.SoulBag;
                            case 2:
                                return ItemClass.HerbBag;
                            case 3:
                                return ItemClass.EnchantingBag;
                            case 4:
                                return ItemClass.EngineeringBag;
                        }
                        break;
                    case 2:
                        switch (Subclass)
                        {
                            case 0:
                                return ItemClass.AxeOneHand;
                            case 1:
                                return ItemClass.AxeTwoHand;
                            case 2:
                                return ItemClass.Bow;
                            case 3:
                                return ItemClass.Gun;
                            case 4:
                                return ItemClass.MaceOneHand;
                            case 5:
                                return ItemClass.MaceTwoHand;
                            case 6:
                                return ItemClass.Polearm;
                            case 7:
                                return ItemClass.SwordOneHand;
                            case 8:
                                return ItemClass.SwordTwoHand;
                            case 10:
                                return ItemClass.Staff;
                            case 13:
                                return ItemClass.Fist;
                            case 14:
                                return ItemClass.MiscWeapon;
                            case 15:
                                return ItemClass.Dagger;
                            case 16:
                                return ItemClass.Thrown;
                            case 17:
                                return ItemClass.Spear;
                            case 18:
                                return ItemClass.Crossbow;
                            case 19:
                                return ItemClass.Wand;
                            case 20:
                                return ItemClass.FishingPole;
                        }
                        break;
                    case 4:
                        switch (Subclass)
                        {
                            case 0:
                                return ItemClass.MiscArmor;
                            case 1:
                                return ItemClass.Cloth;
                            case 2:
                                return ItemClass.Leather;
                            case 3:
                                return ItemClass.Mail;
                            case 4:
                                return ItemClass.Plate;
                            case 6:
                                return ItemClass.Shield;
                            case 7:
                                return ItemClass.Libram;
                            case 8:
                                return ItemClass.Idol;
                            case 9:
                                return ItemClass.Totem;
                        }
                        break;
                    case 5:
                        switch (Subclass)
                        {
                            case 0:
                                return ItemClass.Reagent;
                        }
                        break;
                    case 6:
                        switch (Subclass)
                        {
                            case 2:
                                return ItemClass.Arrow;
                            case 3:
                                return ItemClass.Bullet;
                        }
                        break;
                    case 7:
                        switch (Subclass)
                        {
                            case 0:
                                return ItemClass.TradeGood;
                            case 1:
                                return ItemClass.Parts;
                            case 2:
                                return ItemClass.Explosives;
                            case 3:
                                return ItemClass.Devices;
                        }
                        break;
                    case 9:
                        switch (Subclass)
                        {
                            case 0:
                                return ItemClass.ClassBook;
                            case 1:
                                return ItemClass.LeatherworkingRecipe;
                            case 2:
                                return ItemClass.TailoringRecipe;
                            case 3:
                                return ItemClass.EngineeringRecipe;
                            case 4:
                                return ItemClass.BlacksmithingRecipe;
                            case 5:
                                return ItemClass.CookingRecipe;
                            case 6:
                                return ItemClass.AlchemyRecipe;
                            case 7:
                                return ItemClass.FirstAidRecipe;
                            case 8:
                                return ItemClass.EnchantingRecipe;
                            case 9:
                                return ItemClass.FishingRecipe;
                        }
                        break;
                    case 11:
                        switch (Subclass)
                        {
                            case 2:
                                return ItemClass.Quiver;
                            case 3:
                                return ItemClass.AmmoPouch;
                        }
                        break;
                    case 12:
                        switch (Subclass)
                        {
                            case 0:
                                return ItemClass.Quest;
                        }
                        break;
                    case 13:
                        switch (Subclass)
                        {
                            case 0:
                                return ItemClass.Key;
                            case 1:
                                return ItemClass.Lockpick;
                        }
                        break;
                }
                return ItemClass.Junk;
            }
        }

        public float GetWeaponGearScore()
        {
            float itemWeightedAverage = ItemLevel * ItemLevelWeight;

            itemWeightedAverage += Armor * ArmorWeight;
            itemWeightedAverage += Block * BlockWeight;

            itemWeightedAverage += RangedModRange * RangeModWeight;

            itemWeightedAverage += DmgMin1 * DmgMin1Weight;
            itemWeightedAverage += DmgMin2 * DmgMin2Weight;
            itemWeightedAverage += DmgMin3 * DmgMin3Weight;
            itemWeightedAverage += DmgMin4 * DmgMin4Weight;
            itemWeightedAverage += DmgMin5 * DmgMin5Weight;

            itemWeightedAverage += DmgMax1 * DmgMax1Weight;
            itemWeightedAverage += DmgMax2 * DmgMax2Weight;
            itemWeightedAverage += DmgMax3 * DmgMax3Weight;
            itemWeightedAverage += DmgMax4 * DmgMax4Weight;
            itemWeightedAverage += DmgMax5 * DmgMax5Weight;

            itemWeightedAverage += StatValue1 * GetStatWeightByType(StatType1);
            itemWeightedAverage += StatValue2 * GetStatWeightByType(StatType2);
            itemWeightedAverage += StatValue3 * GetStatWeightByType(StatType3);
            itemWeightedAverage += StatValue4 * GetStatWeightByType(StatType4);
            itemWeightedAverage += StatValue5 * GetStatWeightByType(StatType5);
            itemWeightedAverage += StatValue6 * GetStatWeightByType(StatType6);
            itemWeightedAverage += StatValue7 * GetStatWeightByType(StatType7);
            itemWeightedAverage += StatValue8 * GetStatWeightByType(StatType8);
            itemWeightedAverage += StatValue9 * GetStatWeightByType(StatType9);
            itemWeightedAverage += StatValue10 * GetStatWeightByType(StatType10);

            return itemWeightedAverage;
        }

        public float GetArmorGearScore()
        {
            float itemWeightedAverage = ItemLevel * ItemLevelWeight;

            itemWeightedAverage += Armor * ArmorWeight;
            itemWeightedAverage += Block * BlockWeight;

            itemWeightedAverage += StatValue1 * GetStatWeightByType(StatType1);
            itemWeightedAverage += StatValue2 * GetStatWeightByType(StatType2);
            itemWeightedAverage += StatValue3 * GetStatWeightByType(StatType3);
            itemWeightedAverage += StatValue4 * GetStatWeightByType(StatType4);
            itemWeightedAverage += StatValue5 * GetStatWeightByType(StatType5);
            itemWeightedAverage += StatValue6 * GetStatWeightByType(StatType6);
            itemWeightedAverage += StatValue7 * GetStatWeightByType(StatType7);
            itemWeightedAverage += StatValue8 * GetStatWeightByType(StatType8);
            itemWeightedAverage += StatValue9 * GetStatWeightByType(StatType9);
            itemWeightedAverage += StatValue10 * GetStatWeightByType(StatType10);

            return itemWeightedAverage;
        }
        public static float GetStatWeightByType(int statType)
        {
            switch (statType)
            {
                case (int)StatType.Agility:
                    return AgilityWeight;
                case (int)StatType.Strength:
                    return StrengthWeight;
                case (int)StatType.Intellect:
                    return IntellectWeight;
                case (int)StatType.Spirit:
                    return SpiritWeight;
                case (int)StatType.Stamina:
                    return StaminaWeight;
            }

            return 1.0f;
        }
    }
    public enum ItemClass
    {
        Consumable,
        Bag,
        SoulBag,
        HerbBag,
        EnchantingBag,
        EngineeringBag,
        AxeOneHand,
        AxeTwoHand,
        Bow,
        Gun,
        MaceOneHand,
        MaceTwoHand,
        Polearm,
        SwordOneHand,
        SwordTwoHand,
        Staff,
        Fist,
        MiscWeapon,
        Dagger,
        Thrown,
        Spear,
        Crossbow,
        Wand,
        FishingPole,
        MiscArmor,
        Cloth,
        Leather,
        Mail,
        Plate,
        Shield,
        Libram,
        Idol,
        Totem,
        Reagent,
        Arrow,
        Bullet,
        TradeGood,
        Parts,
        Explosives,
        Devices,
        ClassBook,
        LeatherworkingRecipe,
        TailoringRecipe,
        EngineeringRecipe,
        BlacksmithingRecipe,
        CookingRecipe,
        AlchemyRecipe,
        FirstAidRecipe,
        EnchantingRecipe,
        FishingRecipe,
        Quiver,
        AmmoPouch,
        Quest,
        Key,
        Lockpick,
        Junk
    }
    public enum StatType
    {
        NoStats = 0,
        Health = 1,
        Agility = 3,
        Strength = 4,
        Intellect = 5,
        Spirit = 6,
        Stamina = 7,
    }
    public enum DamageType
    {
        Physical = 0,
        Holy = 1,
        Fire = 2,
        Nature = 3,
        Frost = 4,
        Shadow = 5,
        Arcane = 6
    }
    public enum SpellTrigger
    {
        OnUse = 0,
        OnEquip = 1,
        ChanceOnHit = 2,
        Soulstone = 4,
        OnUseWithoutDelay = 5
    }
    public enum Bonding
    {
        NoBinding = 0,
        BindOnPickup = 1,
        BindOnEquip = 2,
        BindOnUse = 3,
        QuestItem = 4
    }
    public enum Material
    {
        NoBinding = 0,
        BindOnPickup = 1,
        BindOnEquip = 2,
        BindOnUse = 3,
        QuestItem = 4
    }
}
