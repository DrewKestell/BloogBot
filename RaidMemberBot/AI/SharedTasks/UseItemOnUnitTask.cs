using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
{
    public class UseItemOnUnitTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;

        readonly WoWUnit targetUnit;
        readonly WoWItem usableItem;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        int stuckCount;
        bool itemUsed;

        public UseItemOnUnitTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target, WoWItem wowItem)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.targetUnit = target;
            this.usableItem = wowItem;
            itemUsed = false;
            player = ObjectManager.Instance.Player;
            stuckHelper = new StuckHelper(container, botTasks);
        }

        public void Update()
        {
            if (stuckHelper.CheckIfStuck())
                stuckCount++;

            if (player.Location.GetDistanceTo(targetUnit.Location) < 3 || stuckCount > 20)
            {
                if (!itemUsed)
                {
                    player.SetTarget(targetUnit.Guid);
                    player.StopMovement(Constants.Enums.ControlBits.Nothing);
                    usableItem.Use();

                    itemUsed = true;
                } else if (Wait.For("ItemUseAnim", 1000))
                {
                    botTasks.Pop();
                    return;
                }
            }
            else
            {
                var nextWaypoint = Navigation.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, player.Location, targetUnit.Location, false);
                player.MoveToward(nextWaypoint[0]);
            }

        }
    }
}
