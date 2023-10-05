// Friday owns this file!

using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace BeastMasterHunterBot
{
    class CombatTask : CombatRotationTask, IBotTask
    {
        const string AutoAttackLuaScript = "if IsCurrentAction('84') == nil then CastSpellByName('Attack') end";
        const string GunLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Auto Shot') end"; // 8-35 yards
        const string LosErrorMessage = "Target not in line of sight";
        const string OutOfAmmoErrorMessage = "Ammo needs to be in the paper doll ammo slot before it can be fired";

        const string RaptorStrike = "Raptor Strike";
        const string ArcaneShot = "Arcane Shot";
        const string SerpentSting = "Serpent Sting";
        const string MultiShot = "Multi-Shot";
        const string ImmolationTrap = "Immolation Trap";
        const string MongooseBite = "Mongoose Bite";
        const string HuntersMark = "Hunter's Mark";
        const string Parry = "Parry";
        const string RapidFire = "Rapid Fire";
        const string ConcussiveShot = "Concussive Shot";
        const string ScareBeast = "Scare Beast";
        const string AspectOfTheHawk = "Aspect of the Hawk";
        const string CallPet = "Call Pet";
        const string MendPet = "Mend Pet";
        const string DistractingShot = "Distracting Shot";
        const string WingClip = "Wing Clip";

        readonly WoWUnit target;
        readonly LocalPlayer player;
        //readonly pet;

        internal CombatTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> targets) : base(container, botTasks, targets, 30)
        {
            player = ObjectManager.Instance.Player;
            this.target = targets[0];
        }

        public new void Update()
        {
            if (base.Update())
                return;

            var gun = Inventory.Instance.GetEquippedItem(EquipSlot.Ranged);
            var canUseRanged = gun != null && player.Location.GetDistanceTo(target.Location) > 5 && player.Location.GetDistanceTo(target.Location) < 34;
            if (gun == null)
            {
                Lua.Instance.Execute(AutoAttackLuaScript);
            }
            else if (canUseRanged && player.ManaPercent < 60)
            {
                Lua.Instance.Execute(GunLuaScript);
            } 
            else if (gun != null && canUseRanged)
            {
                //if (!target.HasDebuff(HuntersMark)) 
                //{
                //     TryCastSpell(HuntersMark, 0, 34);
                //}
                //else 
                if (!target.HasDebuff(SerpentSting))
                { 
                    TryCastSpell(SerpentSting, 0, 34);
                }
                else if (player.ManaPercent > 60)
                {
                    TryCastSpell(ArcaneShot, 0, 34);
                }
                return;


                //TryCastSpell(ConcussiveShot, 0, 34);
            }
            else
            {
            // melee rotation
                TryCastSpell(RaptorStrike, 0, 5);
            }
        }
    }
}
