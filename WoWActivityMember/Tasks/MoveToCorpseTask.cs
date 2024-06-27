using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using static WoWActivityMember.Constants.Enums;

namespace WoWActivityMember.Tasks.SharedStates
{
    public class MoveToCorpseTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        bool walkingOnWater;
        readonly int stuckCount;

        bool initialized;

        public void Update()
        {
            if (!initialized)
            {
                initialized = true;
            }

            if (stuckCount == 10)
            {
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

            Objects.Position[] nextWaypoint = Navigation.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.CorpsePosition, true);

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
