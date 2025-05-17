using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWLocalPlayer(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.Player) : WoWUnit(highGuid, objectType), IWoWLocalPlayer
    {
        public Position CorpsePosition => throw new NotImplementedException();
        public bool InGhostForm => throw new NotImplementedException();
        public bool IsCursed => throw new NotImplementedException();
        public bool IsPoisoned => throw new NotImplementedException();
        public int ComboPoints => throw new NotImplementedException();
        public bool IsDiseased => throw new NotImplementedException();
        public string CurrentStance => throw new NotImplementedException();
        public bool HasMagicDebuff => throw new NotImplementedException();
        public bool TastyCorpsesNearby => throw new NotImplementedException();
        public bool CanRiposte => throw new NotImplementedException();
        public bool MainhandIsEnchanted => throw new NotImplementedException();
        public uint Copper => throw new NotImplementedException();
        public bool IsAutoAttacking => throw new NotImplementedException();
        public bool CanResurrect => throw new NotImplementedException();
        public Race Race => throw new NotImplementedException();
        public Class Class => throw new NotImplementedException();
        public Gender Gender => throw new NotImplementedException();
        public bool IsDrinking => throw new NotImplementedException();
        public bool IsEating => throw new NotImplementedException();
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
        public void AcceptResurrect()
        {
            throw new NotImplementedException();
        }

        public bool CanCastSpell(int spellId, ulong targetGuid)
        {
            throw new NotImplementedException();
        }

        public void CastSpell(string spellName, int rank = -1, bool castOnSelf = false)
        {
            throw new NotImplementedException();
        }

        public void CastSpell(int spellId, int rank = -1, bool castOnSelf = false)
        {
            throw new NotImplementedException();
        }

        public void DestroyItemInContainer(int bagSlot, int slotId, int quantity = -1)
        {
            throw new NotImplementedException();
        }

        public void DoEmote(Emote emote)
        {
            throw new NotImplementedException();
        }

        public void DoEmote(TextEmote emote)
        {
            throw new NotImplementedException();
        }

        public void EquipItem(int bagSlot, int slotId, EquipSlot? equipSlot = null)
        {
            throw new NotImplementedException();
        }

        public void Face(Position position)
        {
            throw new NotImplementedException();
        }

        public ulong GetBackpackItemGuid(int parSlot)
        {
            throw new NotImplementedException();
        }

        public uint GetBagGuid(EquipSlot equipSlot)
        {
            throw new NotImplementedException();
        }

        public IWoWItem GetContainedItem(int bagSlot, int slotId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IWoWItem> GetContainedItems()
        {
            throw new NotImplementedException();
        }

        public IWoWItem GetEquippedItem(EquipSlot ranged)
        {
            throw new NotImplementedException();
        }

        public ulong GetEquippedItemGuid(EquipSlot slot)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IWoWItem> GetEquippedItems()
        {
            throw new NotImplementedException();
        }

        public uint GetManaCost(string healingTouch)
        {
            throw new NotImplementedException();
        }

        public bool IsSpellReady(string spellName)
        {
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

        public void MoveToward(Position position, float facing)
        {
            throw new NotImplementedException();
        }

        public void OfferTrade()
        {
            throw new NotImplementedException();
        }

        public void PickupContainedItem(int bagSlot, int slotId, int quantity)
        {
            throw new NotImplementedException();
        }

        public void PlaceItemInContainer(int bagSlot, int slotId)
        {
            throw new NotImplementedException();
        }

        public void RefreshSkills()
        {
            throw new NotImplementedException();
        }

        public void RefreshSpells()
        {
            throw new NotImplementedException();
        }

        public void RetrieveCorpse()
        {
            throw new NotImplementedException();
        }

        public void SetFacing(float facing)
        {
            throw new NotImplementedException();
        }

        public void SetTarget(ulong guid)
        {
            throw new NotImplementedException();
        }

        public void SplitStack(int bag, int slot, int quantity, int destinationBag, int destinationSlot)
        {
            throw new NotImplementedException();
        }

        public void StartMovement(ControlBits bits)
        {
            throw new NotImplementedException();
        }

        public void StopAllMovement()
        {
            throw new NotImplementedException();
        }

        public void StopAttack()
        {
            throw new NotImplementedException();
        }

        public void StopCasting()
        {
            throw new NotImplementedException();
        }

        public void StopMovement(ControlBits bits)
        {
            throw new NotImplementedException();
        }

        public void UnequipItem(EquipSlot slot)
        {
            throw new NotImplementedException();
        }

        public void UseItem(int bagId, int slotId, ulong targetGuid = 0)
        {
            throw new NotImplementedException();
        }
    }
}
