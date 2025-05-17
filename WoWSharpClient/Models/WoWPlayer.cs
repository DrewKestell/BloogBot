using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWPlayer(HighGuid highGuid, WoWObjectType woWObjectType = WoWObjectType.Player) : WoWUnit(highGuid, woWObjectType), IWoWPlayer
    {
        public Race Race { get; set; }
        public Class Class { get; set; }
        public Gender Gender { get; set; }
        public bool IsDrinking { get; set; }
        public bool IsEating { get; set; }
        public HighGuid DuelArbiter { get; } = new HighGuid(new byte[4], new byte[4]);
        public HighGuid ComboTarget { get; } = new HighGuid(new byte[4], new byte[4]);
        public PlayerFlags PlayerFlags { get; set; }
        public uint GuildId { get; set; }
        public uint GuildRank { get; set; }
        public byte[] Bytes { get; } = new byte[4];
        public byte[] Bytes3 { get; } = new byte[4];
        public uint GuildTimestamp { get; set; }
        public QuestSlot[] QuestLog { get; } = [new QuestSlot(), new QuestSlot(), new QuestSlot(), new QuestSlot(), new QuestSlot(),
                                                new QuestSlot(), new QuestSlot(), new QuestSlot(), new QuestSlot(), new QuestSlot(),
                                                new QuestSlot(), new QuestSlot(), new QuestSlot(), new QuestSlot(), new QuestSlot(),
                                                new QuestSlot(), new QuestSlot(), new QuestSlot(), new QuestSlot(), new QuestSlot()];
        public IWoWItem[] VisibleItems { get; } = [new WoWItem(new HighGuid(new byte[4], new byte[4])), new WoWItem(new HighGuid(new byte[4], new byte[4])), new WoWItem(new HighGuid(new byte[4], new byte[4])),
                                                   new WoWItem(new HighGuid(new byte[4], new byte[4])), new WoWItem(new HighGuid(new byte[4], new byte[4])), new WoWItem(new HighGuid(new byte[4], new byte[4])),
                                                   new WoWItem(new HighGuid(new byte[4], new byte[4])), new WoWItem(new HighGuid(new byte[4], new byte[4])), new WoWItem(new HighGuid(new byte[4], new byte[4])),
                                                   new WoWItem(new HighGuid(new byte[4], new byte[4])), new WoWItem(new HighGuid(new byte[4], new byte[4])), new WoWItem(new HighGuid(new byte[4], new byte[4])),
                                                   new WoWItem(new HighGuid(new byte[4], new byte[4])), new WoWItem(new HighGuid(new byte[4], new byte[4])), new WoWItem(new HighGuid(new byte[4], new byte[4])),
                                                   new WoWItem(new HighGuid(new byte[4], new byte[4])), new WoWItem(new HighGuid(new byte[4], new byte[4])), new WoWItem(new HighGuid(new byte[4], new byte[4])), new WoWItem(new HighGuid(new byte[4], new byte[4]))];
        public uint[] Inventory { get; } = new uint[46];
        public uint[] PackSlots { get; } = new uint[32];
        public uint[] BankSlots { get; } = new uint[48];
        public uint[] BankBagSlots { get; } = new uint[12];
        public uint[] VendorBuybackSlots { get; } = new uint[24];
        public uint[] KeyringSlots { get; } = new uint[64];
        public uint Farsight { get; set; }
        public uint XP { get; set; }
        public uint NextLevelXP { get; set; }
        public SkillInfo[] SkillInfo { get; } = [   new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(),
                                                    new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo(), new SkillInfo()];
        public uint CharacterPoints1 { get; set; }
        public uint CharacterPoints2 { get; set; }
        public uint TrackCreatures { get; set; }
        public uint TrackResources { get; set; }
        public uint BlockPercentage { get; set; }
        public uint DodgePercentage { get; set; }
        public uint ParryPercentage { get; set; }
        public uint CritPercentage { get; set; }
        public uint RangedCritPercentage { get; set; }
        public uint[] ExploredZones { get; } = new uint[64];
        public uint RestStateExperience { get; set; }
        public uint Coinage { get; set; }
        public uint[] StatBonusesPos { get; } = new uint[7];
        public uint[] StatBonusesNeg { get; } = new uint[7];
        public uint[] ResistBonusesPos { get; } = new uint[7];
        public uint[] ResistBonusesNeg { get; } = new uint[7];
        public uint[] ModDamageDonePos { get; } = new uint[7];
        public uint[] ModDamageDoneNeg { get; } = new uint[7];
        public float[] ModDamageDonePct { get; } = new float[7];
        public uint AmmoId { get; set; }
        public uint SelfResSpell { get; set; }
        public uint PvpMedals { get; set; }
        public uint[] BuybackPrices { get; } = new uint[12];
        public uint[] BuybackTimestamps { get; } = new uint[12];
        public uint SessionKills { get; set; }
        public uint YesterdayKills { get; set; }
        public uint LastWeekKills { get; set; }
        public uint ThisWeekKills { get; set; }
        public uint ThisWeekContribution { get; set; }
        public uint LifetimeHonorableKills { get; set; }
        public uint LifetimeDishonorableKills { get; set; }
        public uint WatchedFactionIndex { get; set; }
        public uint[] CombatRating { get; } = new uint[20];

        public void OfferTrade()
        {

        }
    }
}
