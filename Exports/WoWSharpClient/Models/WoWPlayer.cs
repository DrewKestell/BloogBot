using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWPlayer(HighGuid highGuid, WoWObjectType woWObjectType = WoWObjectType.Player) : WoWUnit(highGuid, woWObjectType), IWoWPlayer
    {
        public uint MapId { get; set; }
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
        public byte[] PlayerBytes { get; internal set; }
        public byte[] PlayerBytes2 { get; internal set; }
        public byte[] PlayerBytes3 { get; internal set; }
        public byte[] FieldBytes2 { get; internal set; }

        public void OfferTrade()
        {

        }
        public override WoWPlayer Clone()
        {
            var clone = new WoWPlayer(HighGuid, ObjectType);
            clone.CopyFrom(this);
            return clone;
        }

        public override void CopyFrom(WoWObject sourceBase)
        {
            base.CopyFrom(sourceBase);

            if (sourceBase is not WoWPlayer source) return;

            Race = source.Race;
            Class = source.Class;
            Gender = source.Gender;
            IsDrinking = source.IsDrinking;
            IsEating = source.IsEating;
            PlayerFlags = source.PlayerFlags;
            GuildId = source.GuildId;
            GuildRank = source.GuildRank;
            GuildTimestamp = source.GuildTimestamp;
            Farsight = source.Farsight;
            XP = source.XP;
            NextLevelXP = source.NextLevelXP;
            CharacterPoints1 = source.CharacterPoints1;
            CharacterPoints2 = source.CharacterPoints2;
            TrackCreatures = source.TrackCreatures;
            TrackResources = source.TrackResources;
            BlockPercentage = source.BlockPercentage;
            DodgePercentage = source.DodgePercentage;
            ParryPercentage = source.ParryPercentage;
            CritPercentage = source.CritPercentage;
            RangedCritPercentage = source.RangedCritPercentage;
            RestStateExperience = source.RestStateExperience;
            Coinage = source.Coinage;
            AmmoId = source.AmmoId;
            SelfResSpell = source.SelfResSpell;
            PvpMedals = source.PvpMedals;
            SessionKills = source.SessionKills;
            YesterdayKills = source.YesterdayKills;
            LastWeekKills = source.LastWeekKills;
            ThisWeekKills = source.ThisWeekKills;
            ThisWeekContribution = source.ThisWeekContribution;
            LifetimeHonorableKills = source.LifetimeHonorableKills;
            LifetimeDishonorableKills = source.LifetimeDishonorableKills;
            WatchedFactionIndex = source.WatchedFactionIndex;

            Array.Copy(source.Bytes, Bytes, Bytes.Length);
            Array.Copy(source.Bytes3, Bytes3, Bytes3.Length);
            Array.Copy(source.Inventory, Inventory, Inventory.Length);
            Array.Copy(source.PackSlots, PackSlots, PackSlots.Length);
            Array.Copy(source.BankSlots, BankSlots, BankSlots.Length);
            Array.Copy(source.BankBagSlots, BankBagSlots, BankBagSlots.Length);
            Array.Copy(source.VendorBuybackSlots, VendorBuybackSlots, VendorBuybackSlots.Length);
            Array.Copy(source.KeyringSlots, KeyringSlots, KeyringSlots.Length);
            Array.Copy(source.ExploredZones, ExploredZones, ExploredZones.Length);
            Array.Copy(source.StatBonusesPos, StatBonusesPos, StatBonusesPos.Length);
            Array.Copy(source.StatBonusesNeg, StatBonusesNeg, StatBonusesNeg.Length);
            Array.Copy(source.ResistBonusesPos, ResistBonusesPos, ResistBonusesPos.Length);
            Array.Copy(source.ResistBonusesNeg, ResistBonusesNeg, ResistBonusesNeg.Length);
            Array.Copy(source.ModDamageDonePos, ModDamageDonePos, ModDamageDonePos.Length);
            Array.Copy(source.ModDamageDoneNeg, ModDamageDoneNeg, ModDamageDoneNeg.Length);
            Array.Copy(source.ModDamageDonePct, ModDamageDonePct, ModDamageDonePct.Length);
            Array.Copy(source.BuybackPrices, BuybackPrices, BuybackPrices.Length);
            Array.Copy(source.BuybackTimestamps, BuybackTimestamps, BuybackTimestamps.Length);
            Array.Copy(source.CombatRating, CombatRating, CombatRating.Length);

            //for (int i = 0; i < QuestLog.Length; i++)
            //    QuestLog[i] = source.QuestLog[i].Clone();

            //for (int i = 0; i < VisibleItems.Length; i++)
            //    VisibleItems[i] = (IWoWItem)source.VisibleItems[i].Clone();

            //for (int i = 0; i < SkillInfo.Length; i++)
            //    SkillInfo[i] = source.SkillInfo[i].Clone();
        }
    }
}
