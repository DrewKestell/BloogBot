using BotRunner.Constants;
using BotRunner.Interfaces;
using PathfindingService.Models;

namespace BotRunner.Tasks
{
    public class UseItemOnUnitTask(IBotContext botContext, IWoWItem wowItem) : BotTask(botContext), IBotTask
    {
        private readonly IWoWItem usableItem = wowItem;
        private bool itemUsed = false;

        public void Update()
        {
            if (ObjectManager.Player.Position.DistanceTo(ObjectManager.Units.FirstOrDefault(x => x.Guid == ObjectManager.Player.TargetGuid).Position) < 3)
            {
                if (!itemUsed)
                {
                    ObjectManager.Player.SetTarget(ObjectManager.Player.TargetGuid);
                    ObjectManager.Player.StopMovement(ControlBits.Nothing);
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
                Position[] nextWaypoint = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Units.FirstOrDefault(x => x.Guid == ObjectManager.Player.TargetGuid).Position, true);
                ObjectManager.Player.MoveToward(nextWaypoint[0]);
            }
        }
    }
}
