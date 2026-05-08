// Friday owns this file!

using BloogBot;
using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;

namespace BeastMasterHunterBot
{
    class CombatState : CombatStateBase, IBotState
    {
        const string AutoAttackLuaScript = "if IsCurrentAction('84') == nil then CastSpellByName('Attack') end";
        const string GunLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Auto Shot') end"; // 8-35 yards
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

        internal CombatState(
            Stack<IBotState> botStates,
            IDependencyContainer container,
            WoWUnit target,
            bool loot = true) : base(botStates, container, target, 28, loot)
        {
            player = ObjectManager.Player;
            this.target = target;
            //pet = ObjectManager.Pet;
        }

        public new void Update()
        {
            if (base.Update())
                return;

            var pet = ObjectManager.Pet;
            if (pet != null && pet.HealthPercent > 0 && pet.HealthPercent < 75
                && player.Position.DistanceTo(pet.Position) <= 10
                && !pet.HasBuff(MendPet))
            {
                TryCastSpell(MendPet, 0, int.MaxValue);
            }

            var gun = Inventory.GetEquippedItem(EquipSlot.Ranged);
            var canUseRanged = gun != null && player.Position.DistanceTo(target.Position) > 5 && player.Position.DistanceTo(target.Position) < 34;

            if (gun == null)
            {
                player.LuaCall(AutoAttackLuaScript);
            }
            else if (canUseRanged)
            {
                // Proactive LOS check: if we can't see the target, move toward it until we can
                if (!player.InLosWith(target.Position))
                {
                    var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
                    player.MoveToward(nextWaypoint);
                    return;
                }

                // Always keep Auto Shot toggled on during ranged combat
                player.LuaCall(GunLuaScript);

                if (!target.HasDebuff(SerpentSting))
                {
                    TryCastSpell(SerpentSting, 0, 34);
                }
                else if (player.ManaPercent > 60)
                {
                    TryCastSpell(ArcaneShot, 0, 34);
                }
            }
            else
            {
                // melee rotation
                TryCastSpell(RaptorStrike, 0, 5);
            }
        }
    }
}
