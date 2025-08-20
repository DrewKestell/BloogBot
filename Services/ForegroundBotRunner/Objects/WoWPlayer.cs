using ForegroundBotRunner.Mem;
using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace ForegroundBotRunner.Objects
{
    public class WoWPlayer : WoWUnit, IWoWPlayer
    {
        internal WoWPlayer(
            nint pointer,
            HighGuid guid,
            WoWObjectType objectType)
            : base(pointer, guid, objectType)
        {
        }


        public uint MapId
        {
            // this is weird and throws an exception right after entering world,
            // so we catch and ignore the exception to avoid console noise
            get
            {
                try
                {
                    var objectManagerPtr = MemoryManager.ReadIntPtr(Offsets.ObjectManager.ManagerBase);
                    return MemoryManager.ReadUint(nint.Add(objectManagerPtr, 0xCC));
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        public bool IsEating => HasBuff("Food");

        public bool IsDrinking => HasBuff("Drink");

        public Race Race => throw new NotImplementedException();

        public Class Class => throw new NotImplementedException();

        public HighGuid DuelArbiter => throw new NotImplementedException();

        public HighGuid ComboTarget => throw new NotImplementedException();

        public PlayerFlags PlayerFlags => throw new NotImplementedException();

        public uint GuildId => throw new NotImplementedException();

        public uint GuildRank => throw new NotImplementedException();

        public byte[] Bytes => throw new NotImplementedException();

        public byte[] Bytes3 => throw new NotImplementedException();

        public uint GuildTimestamp => throw new NotImplementedException();

        public QuestSlot[] QuestLog => throw new NotImplementedException();

        public IWoWItem[] VisibleItems => throw new NotImplementedException();

        public uint[] Inventory => throw new NotImplementedException();

        public uint[] PackSlots => throw new NotImplementedException();

        public uint[] BankSlots => throw new NotImplementedException();

        public uint[] BankBagSlots => throw new NotImplementedException();

        public uint[] VendorBuybackSlots => throw new NotImplementedException();

        public uint[] KeyringSlots => throw new NotImplementedException();

        public uint Farsight => throw new NotImplementedException();

        public uint XP => throw new NotImplementedException();

        public uint NextLevelXP => throw new NotImplementedException();

        public SkillInfo[] SkillInfo => throw new NotImplementedException();

        public uint CharacterPoints1 => throw new NotImplementedException();

        public uint CharacterPoints2 => throw new NotImplementedException();

        public uint TrackCreatures => throw new NotImplementedException();

        public uint TrackResources => throw new NotImplementedException();

        public uint BlockPercentage => throw new NotImplementedException();

        public uint DodgePercentage => throw new NotImplementedException();

        public uint ParryPercentage => throw new NotImplementedException();

        public uint CritPercentage => throw new NotImplementedException();

        public uint RangedCritPercentage => throw new NotImplementedException();

        public uint[] ExploredZones => throw new NotImplementedException();

        public uint RestStateExperience => throw new NotImplementedException();

        public uint Coinage => throw new NotImplementedException();

        public uint[] StatBonusesPos => throw new NotImplementedException();

        public uint[] StatBonusesNeg => throw new NotImplementedException();

        public uint[] ResistBonusesPos => throw new NotImplementedException();

        public uint[] ResistBonusesNeg => throw new NotImplementedException();

        public uint[] ModDamageDonePos => throw new NotImplementedException();

        public uint[] ModDamageDoneNeg => throw new NotImplementedException();

        public float[] ModDamageDonePct => throw new NotImplementedException();

        public uint AmmoId => throw new NotImplementedException();

        public uint SelfResSpell => throw new NotImplementedException();

        public uint PvpMedals => throw new NotImplementedException();

        public uint[] BuybackPrices => throw new NotImplementedException();

        public uint[] BuybackTimestamps => throw new NotImplementedException();

        public uint SessionKills => throw new NotImplementedException();

        public uint YesterdayKills => throw new NotImplementedException();

        public uint LastWeekKills => throw new NotImplementedException();

        public uint ThisWeekKills => throw new NotImplementedException();

        public uint ThisWeekContribution => throw new NotImplementedException();

        public uint LifetimeHonorableKills => throw new NotImplementedException();

        public uint LifetimeDishonorableKills => throw new NotImplementedException();

        public uint WatchedFactionIndex => throw new NotImplementedException();

        public uint[] CombatRating => throw new NotImplementedException();

        public Gender Gender => throw new NotImplementedException();

        public void OfferTrade()
        {
            throw new NotImplementedException();
        }
    }
}
