using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
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

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;

        WoWUnit target;
        readonly string nuke;
        readonly int range;

        bool frostNovaBackpedaling;
        int frostNovaBackpedalStartTime;
        bool frostNovaJumped;
        bool frostNovaStartedMoving;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;

            if (!Spellbook.Instance.IsSpellReady(Frostbolt))
                nuke = Fireball;
            else if (player.Level >= 8)
                nuke = Frostbolt;
            else if (player.Level >= 6)
                nuke = Fireball;
            else if (player.Level >= 4)
                nuke = Frostbolt;
            else
                nuke = Fireball;

            range = 29 + (player.GetTalentRank(3, 11) * 3);
        }

        public void Update()
        {
            if (frostNovaBackpedaling && !frostNovaStartedMoving && Environment.TickCount - frostNovaBackpedalStartTime > 200)
            {
                player.Turn180();
                player.StartMovement(ControlBits.Front);
                frostNovaStartedMoving = true;
            }
            if (frostNovaBackpedaling && !frostNovaJumped && Environment.TickCount - frostNovaBackpedalStartTime > 500)
            {
                player.Jump();
                frostNovaJumped = true;
            }
            if (frostNovaBackpedaling && Environment.TickCount - frostNovaBackpedalStartTime > 2500)
            {
                player.StopMovement(ControlBits.Front);
                player.Face(target.Location);
                frostNovaBackpedaling = false;
            }

            if (frostNovaBackpedaling)
            {
                TryCastSpell(FrostNova); // sometimes we try to cast too early and get into this state while FrostNova is still ready.
                return;
            }

            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                botTasks.Pop();
                return;
            }

            if (target == null || target.HealthPercent <= 0)
            {
                target = ObjectManager.Instance.Aggressors.First();
            }

            if (base.Update(target, 29 + (ObjectManager.Instance.Player.GetTalentRank(3, 11) * 3)))
                return;

            TryCastSpell(Evocation, 0, int.MaxValue, (player.HealthPercent > 50 || player.HasBuff(IceBarrier)) && player.ManaPercent < 8 && target.HealthPercent > 15);

            var wand = Inventory.Instance.GetEquippedItem(EquipSlot.Ranged);
            if (wand != null && player.ManaPercent <= 10 && player.IsCasting && player.Channeling == 0)
                Lua.Instance.Execute(WandLuaScript);
            else
            {
                TryCastSpell(SummonWaterElemental, !ObjectManager.Instance.Units.Any(u => u.Name == "Water Elemental" && u.SummonedByGuid == player.Guid));

                TryCastSpell(ColdSnap, !Spellbook.Instance.IsSpellReady(SummonWaterElemental));

                TryCastSpell(IcyVeins, ObjectManager.Instance.Aggressors.Count() > 1);

                TryCastSpell(FireWard, 0, int.MaxValue, FireWardTargets.Any(c => target.Name.Contains(c)) && (target.HealthPercent > 20 || player.HealthPercent < 10));

                TryCastSpell(FrostWard, 0, int.MaxValue, FrostWardTargets.Any(c => target.Name.Contains(c)) && (target.HealthPercent > 20 || player.HealthPercent < 10));

                TryCastSpell(Counterspell, 0, 30, target.Mana > 0 && target.IsCasting);

                TryCastSpell(IceBarrier, 0, 50, !player.HasBuff(IceBarrier) && (ObjectManager.Instance.Aggressors.Count() >= 2 || (!Spellbook.Instance.IsSpellReady(FrostNova) && player.HealthPercent < 95 && player.ManaPercent > 40 && (target.HealthPercent > 20 || player.HealthPercent < 10))));

                TryCastSpell(FrostNova, 0, 9, target.TargetGuid == player.Guid && (target.HealthPercent > 20 || player.HealthPercent < 30) && !IsTargetFrozen && !ObjectManager.Instance.Units.Any(u => u.Guid != target.Guid && u.HealthPercent > 0 && u.Guid != player.Guid && u.Location.GetDistanceTo(player.Location) <= 12), callback: FrostNovaCallback);

                TryCastSpell(ConeOfCold, 0, 8, player.Level >= 30 && target.HealthPercent > 20 && IsTargetFrozen);

                TryCastSpell(FireBlast, 0, 20, !IsTargetFrozen);

                // Either Frostbolt or Fireball depending on what is stronger. Will always use Frostbolt at level 8+.
                TryCastSpell(nuke, 0, range);
            }
        }

        Action FrostNovaCallback => () =>
        {
            frostNovaStartedMoving = false;
            frostNovaJumped = false;
            frostNovaBackpedaling = true;
            frostNovaBackpedalStartTime = Environment.TickCount;
        };

        bool IsTargetFrozen => target.HasDebuff(Frostbite) || target.HasDebuff(FrostNova);
    }
}
