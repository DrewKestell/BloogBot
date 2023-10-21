using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace ProtectionWarriorBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
    {
        const string BattleStance = "Battle Stance";
        const string DefensiveStance = "Defensive Stance";
        const string BerserkerStance = "Berserker Stance";

        const string BattleShout = "Battle Shout";
        const string Berserking = "Berserking";
        const string Bloodrage = "Bloodrage";
        const string ConcussionBlow = "Concussion Blow";
        const string DemoralizingShout = "Demoralizing Shout";
        const string Execute = "Execute";
        const string HeroicStrike = "Heroic Strike";
        const string LastStand = "Last Stand";
        const string Overpower = "Overpower";
        const string Rend = "Rend";
        const string Retaliation = "Retaliation";
        const string Revenge = "Revenge";
        const string ShieldBash = "Shield Bash";
        const string ShieldSlam = "Shield Slam";
        const string SunderArmor = "Sunder Armor";
        const string Taunt = "Taunt";
        const string ThunderClap = "Thunder Clap";

        readonly Stopwatch stopwatch = new Stopwatch();
        WoWUnit target;
        Location tankSpot;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            tankSpot = Container.Player.Location;

            WoWEventHandler.Instance.OnBlockParryDodge += Instance_OnBlockParryDodge;
        }

        private void Instance_OnBlockParryDodge(object sender, EventArgs e)
        {
            stopwatch.Restart();
        }

        public void Update()
        {
            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Instance.Aggressors.Any(x => x.TargetGuid != Container.Player.Guid))
            {
                target = ObjectManager.Instance.Aggressors.FindAll(x => x.TargetGuid != Container.Player.Guid).First();
                Container.Player.SetTarget(target);

                TryUseAbility(Taunt);
            }
            else if (target == null)
            {
                target = ObjectManager.Instance.Aggressors.First();
                Container.Player.SetTarget(target);
            }

            if (ObjectManager.Instance.Aggressors.All(x => x.TargetGuid == Container.Player.Guid))
            {

                if (tankSpot.DistanceToPlayer() > 3)
                {
                    Location[] locations = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, tankSpot, true);

                    if (locations.Where(loc => Container.Player.InLosWith(loc)).Count() > 1)
                    {
                        Container.Player.MoveToward(locations.Where(loc => Container.Player.InLosWith(loc)).ToArray()[1]);
                    }
                    else
                    {
                        Container.Player.StopAllMovement();
                    }
                } else
                {
                    Container.Player.StopAllMovement();
                    // ensure we're facing the target
                    if (!Container.Player.IsFacing(target.Location))
                        Container.Player.Face(target.Location);
                }
            } else
            {
                Update(target, 3);
            }

            TryUseAbility(Bloodrage, condition: Container.HostileTarget.HealthPercent > 50);

            if (ObjectManager.Instance.Aggressors.Count() >= 3)
            {
                TryUseAbility(Retaliation);
            }
            if (ObjectManager.Instance.Aggressors.Count() >= 4 && !ObjectManager.Instance.Aggressors.All(u => u.HasDebuff(ThunderClap)))
            {
                if (Container.Player.CurrentStance != BattleStance)
                {
                    TryCastSpell(BattleStance);
                }

                TryUseAbility(ThunderClap, 20, !target.HasDebuff(ThunderClap));

                TryUseAbility(Overpower, 5, Container.Player.CurrentStance == BattleStance && Container.Player.CanOverpower);
            }
            else
            {
                if (Container.Player.CurrentStance != DefensiveStance)
                {
                    TryCastSpell(DefensiveStance);
                }

                TryUseAbility(Revenge, 5, Container.Player.CurrentStance == DefensiveStance && stopwatch.IsRunning);

                TryUseAbility(ShieldBash, 10, Container.HostileTarget.IsCasting && Container.HostileTarget.Mana > 0);

                TryUseAbility(Rend, 10, !target.HasDebuff(Rend) && Container.HostileTarget.HealthPercent > 50 && Container.HostileTarget.CreatureType != CreatureType.Elemental && Container.HostileTarget.CreatureType != CreatureType.Undead);

                TryUseAbility(ShieldSlam, 20, Container.HostileTarget.HealthPercent > 30);

                TryUseAbility(SunderArmor, 15);

                TryUseAbility(HeroicStrike, 40, Container.HostileTarget.HealthPercent > 40 && !Container.Player.IsCasting);
            }

            TryUseAbility(DemoralizingShout, 10, !target.HasDebuff(DemoralizingShout));

            TryUseAbility(LastStand, condition: Container.Player.HealthPercent <= 8);

            TryUseAbility(Berserking, 5, Container.Player.CurrentStance == BerserkerStance && Container.Player.HealthPercent < 30);

            TryUseAbility(BattleShout, 10, !Container.Player.HasBuff(BattleShout));

            TryUseAbility(ConcussionBlow, 15, !target.IsStunned && Container.HostileTarget.HealthPercent > 40);

            TryUseAbility(Execute, 20, Container.HostileTarget.HealthPercent < 20);

            if (stopwatch.ElapsedMilliseconds > 5000)
            {
                stopwatch.Stop();
            }
        }
    }
}
