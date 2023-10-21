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
            else if (Container.Player.Level >= 8)
                nuke = Frostbolt;
            else if (Container.Player.Level >= 6)
                nuke = Fireball;
            else if (Container.Player.Level >= 4)
                nuke = Frostbolt;
            else
                nuke = Fireball;

            range = 29 + (Container.Player.GetTalentRank(3, 11) * 3);
        }

        public void Update()
        {
            if (frostNovaBackpedaling && !frostNovaStartedMoving && Environment.TickCount - frostNovaBackpedalStartTime > 200)
            {
                Container.Player.Turn180();
                Container.Player.StartMovement(ControlBits.Front);
                frostNovaStartedMoving = true;
            }
            if (frostNovaBackpedaling && !frostNovaJumped && Environment.TickCount - frostNovaBackpedalStartTime > 500)
            {
                Container.Player.Jump();
                frostNovaJumped = true;
            }
            if (frostNovaBackpedaling && Environment.TickCount - frostNovaBackpedalStartTime > 2500)
            {
                Container.Player.StopMovement(ControlBits.Front);
                Container.Player.Face(target.Location);
                frostNovaBackpedaling = false;
            }

            if (frostNovaBackpedaling)
            {
                TryCastSpell(FrostNova); // sometimes we try to cast too early and get into this state while FrostNova is still ready.
                return;
            }

            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (target == null || Container.HostileTarget.HealthPercent <= 0)
            {
                target = ObjectManager.Instance.Aggressors.First();
            }

            if (base.Update(target, 29 + (ObjectManager.Instance.Player.GetTalentRank(3, 11) * 3)))
                return;

            TryCastSpell(Evocation, 0, int.MaxValue, (Container.Player.HealthPercent > 50 || Container.Player.HasBuff(IceBarrier)) && Container.Player.ManaPercent < 8 && Container.HostileTarget.HealthPercent > 15);

            var wand = Inventory.Instance.GetEquippedItem(EquipSlot.Ranged);
            if (wand != null && Container.Player.ManaPercent <= 10 && Container.Player.IsCasting && Container.Player.Channeling == 0)
                Lua.Instance.Execute(WandLuaScript);
            else
            {
                TryCastSpell(SummonWaterElemental, !ObjectManager.Instance.Units.Any(u => u.Name == "Water Elemental" && u.SummonedByGuid == Container.Player.Guid));

                TryCastSpell(ColdSnap, !Spellbook.Instance.IsSpellReady(SummonWaterElemental));

                TryCastSpell(IcyVeins, ObjectManager.Instance.Aggressors.Count() > 1);

                TryCastSpell(FireWard, 0, int.MaxValue, FireWardTargets.Any(c => Container.HostileTarget.Name.Contains(c)) && (target.HealthPercent > 20 || Container.Player.HealthPercent < 10));

                TryCastSpell(FrostWard, 0, int.MaxValue, FrostWardTargets.Any(c => Container.HostileTarget.Name.Contains(c)) && (target.HealthPercent > 20 || Container.Player.HealthPercent < 10));

                TryCastSpell(Counterspell, 0, 30, Container.HostileTarget.Mana > 0 && Container.HostileTarget.IsCasting);

                TryCastSpell(IceBarrier, 0, 50, !Container.Player.HasBuff(IceBarrier) && (ObjectManager.Instance.Aggressors.Count() >= 2 || (!Spellbook.Instance.IsSpellReady(FrostNova) && Container.Player.HealthPercent < 95 && Container.Player.ManaPercent > 40 && (target.HealthPercent > 20 || Container.Player.HealthPercent < 10))));

                TryCastSpell(FrostNova, 0, 9, Container.HostileTarget.TargetGuid == Container.Player.Guid && (target.HealthPercent > 20 || Container.Player.HealthPercent < 30) && !IsTargetFrozen && !ObjectManager.Instance.Units.Any(u => u.Guid != Container.HostileTarget.Guid && u.HealthPercent > 0 && u.Guid != Container.Player.Guid && u.Location.GetDistanceTo(Container.Player.Location) <= 12), callback: FrostNovaCallback);

                TryCastSpell(ConeOfCold, 0, 8, Container.Player.Level >= 30 && Container.HostileTarget.HealthPercent > 20 && IsTargetFrozen);

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

        bool IsTargetFrozen => Container.HostileTarget.HasDebuff(Frostbite) || Container.HostileTarget.HasDebuff(FrostNova);
    }
}
