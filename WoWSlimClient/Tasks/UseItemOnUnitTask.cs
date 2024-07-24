using WoWSlimClient.Manager;
using WoWSlimClient.Models;


namespace WoWSlimClient.Tasks.SharedStates
{
    public class UseItemOnUnitTask(IClassContainer container, Stack<IBotTask> botTasks, WoWItem wowItem) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private readonly WoWItem usableItem = wowItem;
        private bool itemUsed = false;

        public void Update()
        {
            if (ObjectManager.Instance.Player.Position.DistanceTo(ObjectManager.Instance.Player.Target.Position) < 3)
            {
                if (!itemUsed)
                {
                    ObjectManager.Instance.Player.SetTarget(ObjectManager.Instance.Player.TargetGuid);
                    ObjectManager.Instance.Player.StopMovement(ControlBits.Nothing);
                    usableItem.Use();

                    itemUsed = true;
                }
                else if (Wait.For("ItemUseAnim", 1000))
                {
                    BotTasks.Pop();
                    return;
                }
            }
            else
            {
                Position[] nextWaypoint = Navigation.Instance.CalculatePath(ObjectManager.Instance.MapId, ObjectManager.Instance.Player.Position, ObjectManager.Instance.Player.Target.Position, true);
                ObjectManager.Instance.Player.MoveToward(nextWaypoint[0]);
            }
        }
    }
}
