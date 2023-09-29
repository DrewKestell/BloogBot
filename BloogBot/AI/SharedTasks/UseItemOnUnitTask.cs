using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Diagnostics;

namespace BloogBot.AI.SharedStates
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

        public UseItemOnUnitTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target, WoWItem wowItem)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.targetUnit = target;
            this.usableItem = wowItem;
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(container, botTasks);
        }

        public void Update()
        {
            if (ScanForQuestUnitsTask.TargetGuidBlacklist.ContainsKey(targetUnit.Guid))
            {
                botTasks.Pop();
                return;
            }

            if (stuckHelper.CheckIfStuck())
                stuckCount++;

            if (player.TargetGuid != targetUnit.Guid)
            {
                player.Target = targetUnit;
            }


            if (player.Position.DistanceTo(targetUnit.Position) < 3 || stuckCount > 20)
            {
                player.StopAllMovement();
                usableItem.Use();

                if (Wait.For("ItemUseAnim", 1000))
                {
                    ScanForQuestUnitsTask.TargetGuidBlacklist.Add(targetUnit.Guid, Stopwatch.StartNew());
                    botTasks.Pop();
                    return;
                }
            }
            else
            {
                var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, targetUnit.Position, false);
                player.MoveToward(nextWaypoint);
            }

        }
    }
}
