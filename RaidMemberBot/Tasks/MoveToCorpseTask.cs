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
                DiscordClientWrapper.SendMessage($"{ObjectManager.Player.Name} is stuck in the MoveToCorpseState. Stopping.");

                while (BotTasks.Count > 0)
                    BotTasks.Pop();

                return;
            }

            if (ObjectManager.Player.Position.DistanceTo2D(ObjectManager.Player.CorpsePosition) < 3)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            Objects.Position[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.CorpsePosition, true);

            if (ObjectManager.Player.Position.Z - nextWaypoint[0].Z > 5)
                walkingOnWater = true;

            if (walkingOnWater)
            {
                if (ObjectManager.Player.MovementFlags != MovementFlags.MOVEFLAG_NONE)
                    ObjectManager.Player.StartMovement(ControlBits.Front);

                if (ObjectManager.Player.Position.Z - nextWaypoint[0].Z < .05)
                {
                    walkingOnWater = false;
                    ObjectManager.Player.StopMovement(ControlBits.Front);
                }
            }

            else
                ObjectManager.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
