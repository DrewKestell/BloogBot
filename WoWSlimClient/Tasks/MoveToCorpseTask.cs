using WoWSlimClient.Manager;
using WoWSlimClient.Models;


namespace WoWSlimClient.Tasks.SharedStates
{
    public class MoveToCorpseTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private bool walkingOnWater;
        private readonly int stuckCount;
        private bool initialized;

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

            if (ObjectManager.Instance.Player.Position.DistanceTo2D(ObjectManager.Instance.Player.CorpsePosition) < 3)
            {
                ObjectManager.Instance.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            Position[] nextWaypoint = Navigation.Instance.CalculatePath(ObjectManager.Instance.MapId, ObjectManager.Instance.Player.Position, ObjectManager.Instance.Player.CorpsePosition, true);

            if (ObjectManager.Instance.Player.Position.Z - nextWaypoint[0].Z > 5)
                walkingOnWater = true;

            if (walkingOnWater)
            {
                if (ObjectManager.Instance.Player.MovementFlags != MovementFlags.MOVEFLAG_NONE)
                    ObjectManager.Instance.Player.StartMovement(ControlBits.Front);

                if (ObjectManager.Instance.Player.Position.Z - nextWaypoint[0].Z < .05)
                {
                    walkingOnWater = false;
                    ObjectManager.Instance.Player.StopMovement(ControlBits.Front);
                }
            }

            else
                ObjectManager.Instance.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
