using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace ArmsWarriorBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull) { }

        public void Update()
        {
            if (Container.HostileTarget.TappedByOther)
            {
                Container.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            if (Container.Player.IsInCombat)
            {
                Container.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(Container, BotTasks));
                return;
            }

            float distanceToTarget = Container.Player.Location.GetDistanceTo(Container.HostileTarget.Location);
            if (distanceToTarget < 25 && distanceToTarget > 8 && Container.Player.IsCasting && Spellbook.Instance.IsSpellReady("Charge") && Container.Player.InLosWith(Container.HostileTarget.Location))
            {
                if (Container.Player.IsCasting)
                {
                        Lua.Instance.Execute($"CastSpellByName('Charge')");
                }
            }

            if (distanceToTarget < 3)
            {
                Container.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(Container, BotTasks));
                return;
            }

            Location[] nextWaypoint = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);
            Container.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
