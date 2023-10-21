using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ProtectionWarriorBot
{
    class RestTask : BotTask, IBotTask
    {
        const int stackCount = 5;

        readonly WoWItem foodItem;
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest) { }

        public void Update()
        {
            if (Container.Player.HealthPercent >= 95 ||
                Container.Player.HealthPercent >= 80 && !Container.Player.IsEating ||                                                                
                ObjectManager.Instance.Player.IsInCombat ||
                ObjectManager.Instance.Units.Any(u => u.TargetGuid == ObjectManager.Instance.Player.Guid))
            {
                Container.Player.Stand();
                BotTasks.Pop();

                var foodCount = foodItem == null ? 0 : Inventory.Instance.GetItemCount(foodItem.Id);
                if (!InCombat && foodCount == 0)
                {
                    var foodToBuy = 28 - (foodCount / stackCount);
                    //var itemsToBuy = new Dictionary<string, int>
                    //{
                    //    { container.BotSettings.Food, foodToBuy }
                    //};

                    //var currentHotspot = container.GetCurrentHotspot();
                    //if (currentHotspot.TravelPath != null)
                    //{
                    //    BotTasks.Push(new TravelState(botTasks, container, currentHotspot.TravelPath.Waypoints, 0));
                    //    BotTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.TravelPath.Waypoints[0]));
                    //}

                    //BotTasks.Push(new BuyItemsState(botTasks, currentHotspot.Innkeeper.Name, itemsToBuy));
                    //BotTasks.Push(new SellItemsState(botTasks, container, currentHotspot.Innkeeper.Name));
                    //BotTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.Innkeeper.Location));
                    //container.CheckForTravelPath(botTasks, true, false);
                }

                return;
            }

            if (foodItem != null && !ObjectManager.Instance.Player.IsEating)
                foodItem.Use();
        }

        bool InCombat => ObjectManager.Instance.Player.IsInCombat || ObjectManager.Instance.Units.Any(u => u.TargetGuid == ObjectManager.Instance.Player.Guid);
    }
}
