

using WoWActivityMember.Tasks;
using WoWActivityMember.Tasks.SharedStates;
using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;
using static WoWActivityMember.Constants.Enums;

namespace HunterBeastMastery.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {
        private const string AutoAttackLuaScript = "if IsCurrentAction('84') == nil then CastSpellByName('Attack') end";
        private const string GunLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Auto Shot') end"; // 8-35 yards
        private const string LosErrorMessage = "Target not in line of sight";
        private const string OutOfAmmoErrorMessage = "Ammo needs to be in the paper doll ammo slot before it can be fired";
        private const string RaptorStrike = "Raptor Strike";
        private const string ArcaneShot = "Arcane Shot";
        private const string SerpentSting = "Serpent Sting";
        private const string MultiShot = "Multi-Shot";
        private const string ImmolationTrap = "Immolation Trap";
        private const string MongooseBite = "Mongoose Bite";
        private const string HuntersMark = "Hunter's Mark";
        private const string Parry = "Parry";
        private const string RapidFire = "Rapid Fire";
        private const string ConcussiveShot = "Concussive Shot";
        private const string ScareBeast = "Scare Beast";
        private const string AspectOfTheHawk = "Aspect of the Hawk";
        private const string CallPet = "Call Pet";
        private const string MendPet = "Mend Pet";
        private const string DistractingShot = "Distracting Shot";
        private const string WingClip = "Wing Clip";

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        public override void PerformCombatRotation()
        {

        }

        public void Update()
        {
            if (ObjectManager.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Target == null || ObjectManager.Player.Target.HealthPercent <= 0)
            {
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors[0].Guid);
            }

            if (Update(28))
                return;

            ObjectManager.Player.StopAllMovement();

            WoWItem gun = Inventory.GetEquippedItem(EquipSlot.Ranged);
            bool canUseRanged = gun != null && ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) > 5 && ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) < 34;
            if (gun == null)
            {
                Functions.LuaCall(AutoAttackLuaScript);
            }
            else if (canUseRanged && ObjectManager.Player.ManaPercent < 60)
            {
                Functions.LuaCall(GunLuaScript);
            }
            else if (gun != null && canUseRanged)
            {
                //if (!target.HasDebuff(HuntersMark)) 
                //{
                //     TryCastSpell(HuntersMark, 0, 34);
                //}
                //else 
                if (!ObjectManager.Player.Target.HasDebuff(SerpentSting))
                {
                    TryCastSpell(SerpentSting, 0, 34);
                }
                else if (ObjectManager.Player.ManaPercent > 60)
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
