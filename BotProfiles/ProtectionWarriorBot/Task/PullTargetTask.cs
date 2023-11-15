using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System;
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

            if (ObjectManager.Player.IsInCombat)
            {
                ObjectManager.Player.StartMovement(ControlBits.Back);
            }

            if (ObjectManager.Player.IsCasting || ObjectManager.Aggressors.Count > 0)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(Container.CreatePvERotationTask(Container, BotTasks));
                return;
            }

            if (Container.HostileTarget.Health == 0 || (!ObjectManager.Player.InLosWith(Container.HostileTarget.Position) || !ObjectManager.Player.InLosWith(Container.CurrentWaypoint)) && Wait.For("LosTimer", 2000))
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(Container.HostileTarget.Position);

            if (distanceToTarget < 25 && distanceToTarget > 8 && ObjectManager.Player.InLosWith(Container.HostileTarget.Position))
            {
                ObjectManager.Player.StopAllMovement();

                if (!ObjectManager.Player.IsCasting)
                    Functions.LuaCall("CastSpellByName('Shoot Bow')");

                if (distanceToTarget < 5)
                {
                    BotTasks.Pop();
                    BotTasks.Push(Container.CreatePvERotationTask(Container, BotTasks));
                    return;
                }
            }
            else
            {
                Position[] locations = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, Container.HostileTarget.Position, true);

                if (locations.Where(loc => loc.DistanceTo(ObjectManager.Player.Position) > 3).Count() > 0)
                {
                    Container.CurrentWaypoint = locations.Where(loc => loc.DistanceTo(ObjectManager.Player.Position) > 3).ToArray()[0];
                    ObjectManager.Player.MoveToward(Container.CurrentWaypoint);
                }
                else
                {
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }
            }

            if (distanceToTarget < 3)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(Container, BotTasks));
                return;
            }
        }
    }
}
