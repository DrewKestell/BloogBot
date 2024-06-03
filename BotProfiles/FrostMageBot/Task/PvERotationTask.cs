using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace FrostMageBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
    {
        const string WandLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Shoot') end";

        readonly string[] FireWardTargets = new[] { "Fire", "Flame", "Infernal", "Searing", "Hellcaller", "Dragon", "Whelp" };
        readonly string[] FrostWardTargets = new[] { "Ice", "Frost" };

        const string ColdSnap = "Cold Snap";
        const string ConeOfCold = "Cone of Cold";
        const string Counterspell = "Counterspell";
        const string Evocation = "Evocation";
        const string Fireball = "Fireball";
        const string FireBlast = "Fire Blast";
        const string FireWard = "Fire Ward";
        const string FrostNova = "Frost Nova";
        const string FrostWard = "Frost Ward";
        const string Frostbite = "Frostbite";
        const string Frostbolt = "Frostbolt";
        const string IceBarrier = "Ice Barrier";
        const string IcyVeins = "Icy Veins";
        const string SummonWaterElemental = "Summon Water Elemental";

        readonly string nuke;
        readonly int range;

        bool frostNovaBackpedaling;
        int frostNovaBackpedalStartTime;
        bool frostNovaJumped;
        bool frostNovaStartedMoving;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            if (!ObjectManager.Player.IsSpellReady(Frostbolt))
                nuke = Fireball;
            else if (ObjectManager.Player.Level >= 8)
                nuke = Frostbolt;
            else if (ObjectManager.Player.Level >= 6)
                nuke = Fireball;
            else if (ObjectManager.Player.Level >= 4)
                nuke = Frostbolt;
            else
                nuke = Fireball;

            range = 29 + (ObjectManager.GetTalentRank(3, 11) * 3);
        }

        public void Update()
        {
            if (frostNovaBackpedaling && !frostNovaStartedMoving && Environment.TickCount - frostNovaBackpedalStartTime > 200)
            {
                ObjectManager.Player.Turn180();
                ObjectManager.Player.StartMovement(ControlBits.Front);
                frostNovaStartedMoving = true;
            }
            if (frostNovaBackpedaling && !frostNovaJumped && Environment.TickCount - frostNovaBackpedalStartTime > 500)
            {
                ObjectManager.Player.Jump();
                frostNovaJumped = true;
            }
            if (frostNovaBackpedaling && Environment.TickCount - frostNovaBackpedalStartTime > 2500)
            {
                ObjectManager.Player.StopMovement(ControlBits.Front);
                ObjectManager.Player.Face(ObjectManager.Player.Target.Position);
                frostNovaBackpedaling = false;
            }

            if (frostNovaBackpedaling)
            {
                TryCastSpell(FrostNova); // sometimes we try to cast too early and get into this state while FrostNova is still ready.
                return;
            }

            if (ObjectManager.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Target == null || ObjectManager.Player.Target.HealthPercent <= 0)
            {
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors.First().Guid);
            }

            if (base.Update(29 + (ObjectManager.GetTalentRank(3, 11) * 3)))
                return;

            TryCastSpell(Evocation, 0, int.MaxValue, (ObjectManager.Player.HealthPercent > 50 || ObjectManager.Player.HasBuff(IceBarrier)) && ObjectManager.Player.ManaPercent < 8 && ObjectManager.Player.Target.HealthPercent > 15);

            WoWItem wand = Inventory.GetEquippedItem(EquipSlot.Ranged);
            if (wand != null && ObjectManager.Player.ManaPercent <= 10 && ObjectManager.Player.IsCasting && ObjectManager.Player.ChannelingId == 0)
                Functions.LuaCall(WandLuaScript);
            else
            {
                TryCastSpell(SummonWaterElemental, !ObjectManager.Units.Any(u => u.Name == "Water Elemental" && u.SummonedByGuid == ObjectManager.Player.Guid));

                TryCastSpell(ColdSnap, !ObjectManager.Player.IsSpellReady(SummonWaterElemental));

                TryCastSpell(IcyVeins, ObjectManager.Aggressors.Count() > 1);

                TryCastSpell(FireWard, 0, int.MaxValue, FireWardTargets.Any(c => ObjectManager.Player.Target.Name.Contains(c)) && (ObjectManager.Player.Target.HealthPercent > 20 || ObjectManager.Player.HealthPercent < 10));

                TryCastSpell(FrostWard, 0, int.MaxValue, FrostWardTargets.Any(c => ObjectManager.Player.Target.Name.Contains(c)) && (ObjectManager.Player.Target.HealthPercent > 20 || ObjectManager.Player.HealthPercent < 10));

                TryCastSpell(Counterspell, 0, 30, ObjectManager.Player.Target.Mana > 0 && ObjectManager.Player.Target.IsCasting);

                TryCastSpell(IceBarrier, 0, 50, !ObjectManager.Player.HasBuff(IceBarrier) && (ObjectManager.Aggressors.Count() >= 2 || (!ObjectManager.Player.IsSpellReady(FrostNova) && ObjectManager.Player.HealthPercent < 95 && ObjectManager.Player.ManaPercent > 40 && (ObjectManager.Player.Target.HealthPercent > 20 || ObjectManager.Player.HealthPercent < 10))));

                TryCastSpell(FrostNova, 0, 9, ObjectManager.Player.Target.TargetGuid == ObjectManager.Player.Guid && (ObjectManager.Player.Target.HealthPercent > 20 || ObjectManager.Player.HealthPercent < 30) && !IsTargetFrozen && !ObjectManager.Units.Any(u => u.Guid != ObjectManager.Player.TargetGuid && u.HealthPercent > 0 && u.Guid != ObjectManager.Player.Guid && u.Position.DistanceTo(ObjectManager.Player.Position) <= 12), callback: FrostNovaCallback);

                TryCastSpell(ConeOfCold, 0, 8, ObjectManager.Player.Level >= 30 && ObjectManager.Player.Target.HealthPercent > 20 && IsTargetFrozen);

                TryCastSpell(FireBlast, 0, 20, !IsTargetFrozen);

                // Either Frostbolt or Fireball depending on what is stronger. Will always use Frostbolt at level 8+.
                TryCastSpell(nuke, 0, range);
            }
        }

        public override void PerformCombatRotation()
        {
            throw new NotImplementedException();
        }

        Action FrostNovaCallback => () =>
        {
            frostNovaStartedMoving = false;
            frostNovaJumped = false;
            frostNovaBackpedaling = true;
            frostNovaBackpedalStartTime = Environment.TickCount;
        };

        bool IsTargetFrozen => ObjectManager.Player.Target.HasDebuff(Frostbite) || ObjectManager.Player.Target.HasDebuff(FrostNova);
    }
}
