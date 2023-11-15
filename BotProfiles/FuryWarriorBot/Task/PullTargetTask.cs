using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace FuryWarriorBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull) { }

        public void Update()
        {
            if (Container.HostileTarget.TappedByOther)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsInCombat)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(Container, BotTasks));
                return;
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(Container.HostileTarget.Position);
            if (distanceToTarget < 25 && distanceToTarget > 8 && ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady("Charge") && ObjectManager.Player.InLosWith(Container.HostileTarget.Position))
            {
                if (ObjectManager.Player.IsCasting)
                    Functions.LuaCall("CastSpellByName('Charge')");
            }

            if (distanceToTarget < 3)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(Container, BotTasks));
                return;
            }

            Position[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, Container.HostileTarget.Position, true);
            ObjectManager.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
