using BotRunner.Base;
using BotRunner.Constants;
using BotRunner.Models;

namespace WoWSharpClient.Models
{
    public class Player(HighGuid highGuid) : Unit(highGuid, WoWObjectType.Player)
    {
        public uint GuildId { get; internal set; }
        public HighGuid DuelArbiter { get; } = new HighGuid(new byte[4], new byte[4]);
        public uint GuildRank { get; internal set; }
        public uint DuelTeam { get; internal set; }
        public uint GuildTimestamp { get; internal set; }
        public uint XP { get; internal set; }
        public uint NextLevelXP { get; internal set; }
        public PlayerFlags PlayerFlags { get; internal set; }
        public uint Coinage { get; internal set; }
        public uint RestStateExperience { get; internal set; }
        public QuestSlot[] QuestLog { get; } = [new QuestSlot(),new QuestSlot(),new QuestSlot(),new QuestSlot(),new QuestSlot(),
                                                new QuestSlot(),new QuestSlot(),new QuestSlot(),new QuestSlot(),new QuestSlot(),
                                                new QuestSlot(),new QuestSlot(),new QuestSlot(),new QuestSlot(),new QuestSlot(),
                                                new QuestSlot(),new QuestSlot(),new QuestSlot(),new QuestSlot(),new QuestSlot()];
        public byte[] Bytes { get; } = new byte[4];
        public byte[] Bytes2 { get; } = new byte[4];
        public byte[] Bytes3 { get; } = new byte[4];
        public uint[] BankSlots { get; } = new uint[48];
        public uint[] BankBagSlots { get; } = new uint[12];
        public uint[] BackpackSlots { get; } = new uint[16];
        public uint[] VendorBuybackSlots { get; } = new uint[24];
        public SkillUpdate[] SkillInfo { get; } = [new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(),
                                                    new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate(), new SkillUpdate()];
        public uint[] KeyringSlots { get; } = new uint[64];
        public uint[] CombatRating { get; } = new uint[20];
        public uint[] PackSlots { get; } = new uint[32];
        public Item[] VisibleItems { get; } = [new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),
                                                new Item(new HighGuid(new byte[4], new byte[4])),];
        public uint[] Inventory { get; } = new uint[46];
        public float[] ModDamageDonePct { get; } = new float[7];
        public uint[] ModDamageDoneNeg { get; } = new uint[7];
        public uint[] ModDamageDonePos { get; } = new uint[7];
        public uint[] ResistBonusesNeg { get; } = new uint[7];
        public uint[] ResistBonusesPos { get; } = new uint[7];
        public uint[] StatBonusesNeg { get; } = new uint[7];
        public uint[] StatBonusesPos { get; } = new uint[7];
        public uint[] ExploredZones { get; } = new uint[64];
        public uint AmmoId { get; internal set; }
        public uint SelfResSpell { get; internal set; }
        public uint PvpMedals { get; internal set; }
        public uint[] BuybackPrices { get; } = new uint[12];
        public uint[] BuybackTimestamps { get; } = new uint[12];
        public uint SessionKills { get; internal set; }
        public uint YesterdayKills { get; internal set; }
        public uint LastWeekKills { get; internal set; }
        public uint ThisWeekKills { get; internal set; }
        public uint ThisWeekContribution { get; internal set; }
        public uint LifetimeHonorableKills { get; internal set; }
        public uint Farsight { get; internal set; }
        public uint CharacterPoints1 { get; internal set; }
        public uint CharacterPoints2 { get; internal set; }
        public uint TrackCreatures { get; internal set; }
        public uint TrackResources { get; internal set; }
        public uint BlockPercentage { get; internal set; }
        public uint DodgePercentage { get; internal set; }
        public uint ParryPercentage { get; internal set; }
        public uint CritPercentage { get; internal set; }
        public uint RangedCritPercentage { get; internal set; }
        public HighGuid ComboTarget { get; } = new HighGuid(new byte[4], new byte[4]);
        public uint LifetimeDishonorableKills { get; internal set; }
        public uint WatchedFactionIndex { get; internal set; }
        public bool IsEating => HasBuff("Food");
        public bool IsDrinking => HasBuff("Drink");
    }
}
