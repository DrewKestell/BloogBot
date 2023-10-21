using RaidMemberBot.Constants;
using RaidMemberBot.Game.Statics;
using System;

namespace RaidMemberBot.Objects
{
    /// <summary>
    ///     Class for the local player's pet
    /// </summary>
    public class LocalPet : WoWUnit
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        internal LocalPet(ulong parGuid, IntPtr parPointer, Enums.WoWObjectTypes parType)
            : base(parGuid, parPointer, parType)
        {
        }

        /// <summary>
        ///     Pet should always be friendly
        /// </summary>
        public override Enums.UnitReaction Reaction => Enums.UnitReaction.Friendly;

        /// <summary>
        ///     Feeds the players pet
        /// </summary>
        /// <param name="parFoodName">Pet food name</param>
        public void Feed(string parFoodName)
        {
            if (!GotAura("Feed Pet Effect"))
                if (Inventory.Instance.GetItemCount(parFoodName) != 0)
                {
                    const string checkFeedPet = "{0} = 0; if CursorHasSpell() then CanFeedMyPet = 1 end;";
                    var result = Lua.Instance.ExecuteWithResult(checkFeedPet);
                    if (result[0].Trim().Contains("0"))
                    {
                        const string feedPet = "CastSpellByName('Feed Pet'); TargetUnit('Pet');";
                        Lua.Instance.Execute(feedPet);
                    }
                    const string usePetFood1 =
                        "for bag = 0,4 do for slot = 1,GetContainerNumSlots(bag) do local item = GetContainerItemLink(bag,slot) if item then if string.find(item, '";
                    const string usePetFood2 = "') then PickupContainerItem(bag,slot) break end end end end";
                    Lua.Instance.Execute(usePetFood1 + parFoodName.Replace("'", "\\'") + usePetFood2);
                }
            Lua.Instance.Execute("ClearCursor()");
        }

        /// <summary>
        ///     Let the pet attack your target
        /// </summary>
        public void Attack()
        {
            Lua.Instance.Execute("PetAttack()");
        }

        /// <summary>
        ///     Set pet to follow
        /// </summary>
        public void FollowPlayer()
        {
            Lua.Instance.Execute("PetFollow()");
        }

        /// <summary>
        ///     Dismisses the pet
        /// </summary>
        public void Dismiss()
        {
            Lua.Instance.Execute("PetDismiss()");
        }

        /// <summary>
        ///     Check if the pet is alive
        /// </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            if (ObjectManager.Instance.Player.HasPet && Health > 0)
                return true;
            return false;
        }

        /// <summary>
        ///     Let the pet cast a spell
        /// </summary>
        /// <param name="parPetSpell">Spell name</param>
        public void Cast(string parPetSpell)
        {
            const string castPetSpell1 = "for index = 1,11,1 do curName = GetPetActionInfo(index); if curName == '";
            const string castPetSpell2 = "' then CastPetAction(index); break end end";
            Lua.Instance.Execute(castPetSpell1 + parPetSpell + castPetSpell2);
        }

        /// <summary>
        ///     Can the characters pet use the spell?
        /// </summary>
        /// <param name="parPetSpell">Spell name</param>
        /// <returns></returns>
        public bool CanUse(string parPetSpell)
        {
            const string getPetSpellCd1 =
                "{0} = 0; for index = 1,11,1 do {1} = GetPetActionInfo(index); if {1} == '";
            const string getPetSpellCd2 =
                "' then startTime, duration, enable = GetPetActionCooldown(index); PetSpellEnabled = duration; end end";
            var result = Lua.Instance.ExecuteWithResult(getPetSpellCd1 + parPetSpell + getPetSpellCd2);
            return result[0].Trim().Equals("0");
        }

        /// <summary>
        ///     Determines if the pet is happy
        /// </summary>
        /// <returns></returns>
        public bool IsHappy()
        {
            const string getPetHappiness =
                "happiness, damagePercentage, loyaltyRate = GetPetHappiness(); {0} = happiness;";
            var result = Lua.Instance.ExecuteWithResult(getPetHappiness);
            return result[0].Trim().Equals("3");
        }

        /// <summary>
        ///     Calls the players pet
        /// </summary>
        public void Call()
        {
            Spellbook.Instance.Cast("Call Pet");
        }

        /// <summary>
        ///     Revives the players pet
        /// </summary>
        public void Revive()
        {
            Spellbook.Instance.Cast("Revive Pet");
        }

        /// <summary>
        ///     Is the bots current target attacking our pet
        /// </summary>
        /// <returns>
        ///     returns <c>true</c> if the pet is tanking the Container.HostileTarget. Otherwise <c>false</c>
        /// </returns>
        public bool IsTanking()
        {
            var unit = ObjectManager.Instance.Target;
            if (unit == null) return true;
            return TargetGuid != 0 && Guid == unit.TargetGuid ? true : false;
        }

        /// <summary>
        ///     Determines if the pet is attacking our current target
        /// </summary>
        /// <returns>
        ///     returns <c>true</c> if the pet is attacking our current Container.HostileTarget. <c>false</c> otherwise
        /// </returns>
        public bool IsOnMyTarget()
        {
            var unit = ObjectManager.Instance.Target;
            if (unit == null) return true;
            return TargetGuid == unit.Guid;
        }
    }
}
