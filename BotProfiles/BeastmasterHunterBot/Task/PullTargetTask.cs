// Friday owns this file!

using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BeastMasterHunterBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string GunLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Auto Shot') end";
        const string SerpentSting = "Serpent Sting";
        const string AspectOfTheMonkey = "Aspect Of The Monkey";
        const string AspectOfTheCheetah = "Aspect Of The Cheetah";

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull) { }

        public void Update()
        {
            if (ObjectManager.Instance.Hostiles.Count > 0)
            {
                WoWUnit potentialNewTarget = ObjectManager.Instance.Hostiles.First();

                if (potentialNewTarget != null && potentialNewTarget.Guid != Container.HostileTarget.Guid)
                {
                    Container.HostileTarget = potentialNewTarget;
                    Container.Player.SetTarget(potentialNewTarget);
                }
            }

            if (Container.Player.Location.GetDistanceTo(Container.HostileTarget.Location) < 28)
            {
                Container.Player.StopAllMovement();
                Lua.Instance.Execute(GunLuaScript);
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(Container, BotTasks));
                return;
            } else
            {
                var nextWaypoint = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);
                Container.Player.MoveToward(nextWaypoint[1]);
            }
        }
    }
}
