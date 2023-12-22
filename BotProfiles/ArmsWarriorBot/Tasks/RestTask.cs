using RaidMemberBot.AI;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ArmsWarriorBot
{
    class RestTask : BotTask, IBotTask
    {
        const int stackCount = 5;

        readonly WoWItem foodItem;
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest)
        {
            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            if (ObjectManager.Player.TargetGuid == ObjectManager.Player.Guid)
            {
                if (Inventory.GetEquippedItems().Any(x => x.DurabilityPercentage > 0 && x.DurabilityPercentage < 100))
                {
                    Functions.LuaCall($"SendChatMessage('.repairitems')");
                }
            }
        }

        public void Update()
        {
            if (ObjectManager.Player.HealthPercent >= 95 ||
                ObjectManager.Player.HealthPercent >= 80 && !ObjectManager.Player.IsEating ||                                                                
                ObjectManager.Player.IsInCombat ||
                ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid))
            {
                Wait.RemoveAll();
                ObjectManager.Player.Stand();
                BotTasks.Pop();

                int foodCount = foodItem == null ? 0 : Inventory.GetItemCount(foodItem.ItemId);
                if (!InCombat && foodCount == 0)
                {
                    int foodToBuy = 28 - (foodCount / stackCount);
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
                    //BotTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.Innkeeper.Position));
                    //container.CheckForTravelPath(botTasks, true, false);
                }

                return;
            }

            if (foodItem != null && !ObjectManager.Player.IsEating && Wait.For("EatDelay", 500, true))
                foodItem.Use();
        }

        bool InCombat => ObjectManager.Aggressors.Count() > 0;
    }
}
