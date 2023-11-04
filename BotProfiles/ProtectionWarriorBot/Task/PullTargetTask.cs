using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace ProtectionWarriorBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull) { }

        public void Update()
        {
            if (Container.HostileTarget == null || Container.HostileTarget.HealthPercent == 0 || Container.HostileTarget.Guid == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (Container.Player.IsInCombat)
            {
                Container.Player.StartMovement(ControlBits.Back);
            }

            if (Container.Player.IsCasting || ObjectManager.Instance.Aggressors.Count > 0)
            {
                Container.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(Container.CreatePvERotationTask(Container, BotTasks));
                return;
            }

            if (Container.HostileTarget.Health == 0 || (!Container.Player.InLosWith(Container.HostileTarget) || !Container.Player.InLosWith(Container.CurrentWaypoint)) && Wait.For("LosTimer", 2000))
            {
                Container.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            float distanceToTarget = Container.Player.Location.GetDistanceTo(Container.HostileTarget.Location);

            if (distanceToTarget < 25 && distanceToTarget > 8 && Container.Player.InLosWith(Container.HostileTarget))
            {
                Container.Player.StopAllMovement();

                if (!Container.Player.IsCasting)
                    Lua.Instance.Execute("CastSpellByName('Shoot Bow')");

                if (distanceToTarget < 5)
                {
                    BotTasks.Pop();
                    BotTasks.Push(Container.CreatePvERotationTask(Container, BotTasks));
                    return;
                }
            }
            else
            {
                Location[] locations = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);

                if (locations.Where(loc => loc.DistanceToPlayer() > 3).Count() > 0)
                {
                    Container.CurrentWaypoint = locations.Where(loc => loc.DistanceToPlayer() > 3).ToArray()[0];
                    Container.Player.MoveToward(Container.CurrentWaypoint);
                }
                else
                {
                    Container.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }
            }

            if (distanceToTarget < 3)
            {
                Container.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(Container, BotTasks));
                return;
            }
        }
    }
}
