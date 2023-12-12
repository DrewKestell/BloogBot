using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Client;
using RaidMemberBot.Game;
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

        readonly Stopwatch overpowerStopwatch = new Stopwatch();
        Position tankSpot;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            tankSpot = ObjectManager.Player.Position;

            WoWEventHandler.Instance.OnBlockParryDodge += Instance_OnBlockParryDodge;
        }
        ~PvERotationTask()
        {
            WoWEventHandler.Instance.OnBlockParryDodge -= Instance_OnBlockParryDodge;
        }

        private void Instance_OnBlockParryDodge(object sender, EventArgs e)
        {
            overpowerStopwatch.Restart();
        }

        public void Update()
        {
            try
            {
                if (ObjectManager.Aggressors.Count == 0)
                {
                    Container.HostileTarget = null;
                    BotTasks.Pop();
                    return;
                }

                if (ObjectManager.Aggressors.Any(x => x.TargetGuid != ObjectManager.Player.Guid))
                {
                    Container.HostileTarget = ObjectManager.Aggressors.FindAll(x => x.TargetGuid != ObjectManager.Player.Guid).First();
                    ObjectManager.Player.SetTarget(Container.HostileTarget.Guid);

                    TryUseAbility(Taunt);
                }
                else if (Container.HostileTarget == null)
                {
                    Container.HostileTarget = ObjectManager.Aggressors.First();
                    ObjectManager.Player.SetTarget(Container.HostileTarget.Guid);
                }

                Container.HostileTarget = Container.HostileTarget;

                if (ObjectManager.Aggressors.All(x => x.TargetGuid == ObjectManager.Player.Guid))
                {
                    if (tankSpot.DistanceTo(ObjectManager.Player.Position) > 3)
                    {
                        Position[] locations = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, tankSpot, true);

                        if (locations.Where(loc => ObjectManager.Player.InLosWith(loc)).Count() > 1)
                        {
                            ObjectManager.Player.MoveToward(locations.Where(loc => ObjectManager.Player.InLosWith(loc)).ToArray()[1]);
                        }
                        else
                        {
                            ObjectManager.Player.StopAllMovement();
                        }
                    }
                    else
                    {
                        ObjectManager.Player.StopAllMovement();
                        // ensure we're facing the Container.HostileTarget
                        if (!ObjectManager.Player.IsFacing(Container.HostileTarget.Position))
                            ObjectManager.Player.Face(Container.HostileTarget.Position);
                    }
                }
                else
                {
                    Update(3);
                }

                ObjectManager.Player.StartAttack();

                TryUseAbility(Bloodrage, condition: Container.HostileTarget.HealthPercent > 50);

                if (ObjectManager.Aggressors.Count() >= 3)
                {
                    TryUseAbility(Retaliation);
                }
                if (ObjectManager.Aggressors.Count() >= 4 && !ObjectManager.Aggressors.All(u => u.HasDebuff(ThunderClap)))
                {
                    if (ObjectManager.Player.CurrentStance != BattleStance)
                    {
                        TryCastSpell(BattleStance);
                    }

                    TryUseAbility(ThunderClap, 20, !Container.HostileTarget.HasDebuff(ThunderClap));

                    TryUseAbility(Overpower, 5, ObjectManager.Player.CurrentStance == BattleStance && overpowerStopwatch.IsRunning);
                }
                else
                {
                    if (ObjectManager.Player.CurrentStance != DefensiveStance)
                    {
                        TryCastSpell(DefensiveStance);
                    }

                    TryUseAbility(Revenge, 5, ObjectManager.Player.CurrentStance == DefensiveStance && overpowerStopwatch.IsRunning);

                    TryUseAbility(ShieldBash, 10, Container.HostileTarget.IsCasting && Container.HostileTarget.Mana > 0);

                    TryUseAbility(Rend, 10, !Container.HostileTarget.HasDebuff(Rend) && Container.HostileTarget.HealthPercent > 50 && Container.HostileTarget.CreatureType != CreatureType.Elemental && Container.HostileTarget.CreatureType != CreatureType.Undead);

                    TryUseAbility(ShieldSlam, 20, Container.HostileTarget.HealthPercent > 30);

                    TryUseAbility(SunderArmor, 15);

                    TryUseAbility(HeroicStrike, 40, Container.HostileTarget.HealthPercent > 40 && !ObjectManager.Player.IsCasting);
                }

                TryUseAbility(DemoralizingShout, 10, !Container.HostileTarget.HasDebuff(DemoralizingShout));

                TryUseAbility(LastStand, condition: ObjectManager.Player.HealthPercent <= 8);

                TryUseAbility(Berserking, 5, ObjectManager.Player.CurrentStance == BerserkerStance && ObjectManager.Player.HealthPercent < 30);

                TryUseAbility(BattleShout, 10, !ObjectManager.Player.HasBuff(BattleShout));

                TryUseAbility(ConcussionBlow, 15, !Container.HostileTarget.IsStunned && Container.HostileTarget.HealthPercent > 40);

                TryUseAbility(Execute, 20, Container.HostileTarget.HealthPercent < 20);

                if (overpowerStopwatch.ElapsedMilliseconds > 5000)
                {
                    overpowerStopwatch.Stop();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[PVE ROTATION TASK]{e.Message} {e.StackTrace}");
            }
        }
    }
}
