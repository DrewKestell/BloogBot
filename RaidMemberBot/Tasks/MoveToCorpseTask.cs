using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.AI.SharedStates
{
    public class MoveToCorpseTask : BotTask, IBotTask
    {
        bool walkingOnWater;
        int stuckCount;

        bool initialized;
        
        public MoveToCorpseTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary) { }

        public void Update()
        {
            if (!initialized)
            {
                initialized = true;
            }

            if (stuckCount == 10)
            {
                DiscordClientWrapper.SendMessage($"{Container.Player.Name} is stuck in the MoveToCorpseState. Stopping.");

                while (BotTasks.Count > 0)
                    BotTasks.Pop();

                return;
            }

            if (Container.Player.Location.GetDistanceTo2D(Container.Player.CorpseLocation) < 3)
            {
                Container.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            var nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, Container.Player.Location, Container.Player.CorpseLocation, true);

            if (Container.Player.Location.Z - nextWaypoint[0].Z > 5)
                walkingOnWater = true;

            if (walkingOnWater)
            {
                if (Container.Player.MovementState != MovementFlags.None)
                    Container.Player.StartMovement(ControlBits.Front);

                if (Container.Player.Location.Z - nextWaypoint[0].Z < .05)
                {
                    walkingOnWater = false;
                    Container.Player.StopMovement(ControlBits.Front);
                }
            }

            else
                Container.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
