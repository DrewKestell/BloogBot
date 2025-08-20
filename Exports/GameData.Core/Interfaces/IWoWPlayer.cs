using GameData.Core.Enums;
using GameData.Core.Models;

namespace GameData.Core.Interfaces
{
    public interface IWoWPlayer : IWoWUnit
    {
        uint MapId { get; }
        Race Race { get; }
        Class Class { get; }
        Gender Gender { get; }
        bool IsDrinking { get; }
        bool IsEating { get; }
        HighGuid DuelArbiter { get; }
        HighGuid ComboTarget { get; }
        PlayerFlags PlayerFlags { get; }
        uint GuildId { get; }
        uint GuildRank { get; }
        byte[] Bytes { get; }
        byte[] Bytes3 { get; }
        uint GuildTimestamp { get; }
        QuestSlot[] QuestLog { get; }
        IWoWItem[] VisibleItems { get; }
        uint[] Inventory { get; }
        uint[] PackSlots { get; }
        uint[] BankSlots { get; }
        uint[] BankBagSlots { get; }
        uint[] VendorBuybackSlots { get; }
        uint[] KeyringSlots { get; }
        uint Farsight { get; }
        uint XP { get; }
        uint NextLevelXP { get; }
        SkillInfo[] SkillInfo { get; }
        uint CharacterPoints1 { get; }
        uint CharacterPoints2 { get; }
        uint TrackCreatures { get; }
        uint TrackResources { get; }
        uint BlockPercentage { get; }
        uint DodgePercentage { get; }
        uint ParryPercentage { get; }
        uint CritPercentage { get; }
        uint RangedCritPercentage { get; }
        uint[] ExploredZones { get; }
        uint RestStateExperience { get; }
        uint Coinage { get; }
        uint[] StatBonusesPos { get; }
        uint[] StatBonusesNeg { get; }
        uint[] ResistBonusesPos { get; }
        uint[] ResistBonusesNeg { get; }
        uint[] ModDamageDonePos { get; }
        uint[] ModDamageDoneNeg { get; }
        float[] ModDamageDonePct { get; }
        uint AmmoId { get; }
        uint SelfResSpell { get; }
        uint PvpMedals { get; }
        uint[] BuybackPrices { get; }
        uint[] BuybackTimestamps { get; }
        uint SessionKills { get; }
        uint YesterdayKills { get; }
        uint LastWeekKills { get; }
        uint ThisWeekKills { get; }
        uint ThisWeekContribution { get; }
        uint LifetimeHonorableKills { get; }
        uint LifetimeDishonorableKills { get; }
        uint WatchedFactionIndex { get; }
        uint[] CombatRating { get; }

        void OfferTrade();
    }
}
