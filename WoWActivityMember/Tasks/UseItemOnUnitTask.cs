using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Objects;

namespace WoWActivityMember.Tasks.SharedStates
{
    public class UseItemOnUnitTask(IClassContainer container, Stack<IBotTask> botTasks, WoWItem wowItem) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        readonly WoWItem usableItem = wowItem;

        bool itemUsed = false;

        public void Update()
        {
            if (ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) < 3)
            {
                if (!itemUsed)
                {
                    ObjectManager.Player.SetTarget(ObjectManager.Player.TargetGuid);
                    ObjectManager.Player.StopMovement(Constants.Enums.ControlBits.Nothing);
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
                Position[] nextWaypoint = Navigation.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);
                ObjectManager.Player.MoveToward(nextWaypoint[0]);
            }

        }
    }
}
