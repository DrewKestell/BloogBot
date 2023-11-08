// Friday owns this file!

using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace BeastMasterHunterBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
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

        //readonly pet;
        WoWUnit target;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        public void Update()
        {
            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (Container.HostileTarget == null || Container.HostileTarget.HealthPercent <= 0)
            {
                Container.HostileTarget = ObjectManager.Instance.Aggressors[0];
            }

            if (Update(target, 28))
                return;

            Container.Player.StopAllMovement();

            WoWItem gun = Inventory.Instance.GetEquippedItem(EquipSlot.Ranged);
            bool canUseRanged = gun != null && Container.Player.Location.GetDistanceTo(target.Location) > 5 && Container.Player.Location.GetDistanceTo(target.Location) < 34;
            if (gun == null)
            {
                Lua.Instance.Execute(AutoAttackLuaScript);
            }
            else if (canUseRanged && Container.Player.ManaPercent < 60)
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
                if (!Container.HostileTarget.HasDebuff(SerpentSting))
                { 
                    TryCastSpell(SerpentSting, 0, 34);
                }
                else if (Container.Player.ManaPercent > 60)
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
