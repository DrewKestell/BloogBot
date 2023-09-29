// Nat owns this file!

using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BackstabRogueBot
{
    class RestTask : IBotTask
    {
        const int stackCount = 5;

        const string Cannibalize = "Cannibalize";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;

        readonly WoWItem foodItem;

        public RestTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Player;

            //foodItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Food);
        }

        public void Update()
        {
            if (player.IsChanneling)
                return;

            if (player.HealthPercent >= 95 ||
                player.HealthPercent >= 80 && !player.IsEating ||                                                                
                ObjectManager.Player.IsInCombat ||
                ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid))
            {
                Wait.RemoveAll();
                player.Stand();
                botTasks.Pop();

                var foodCount = foodItem == null ? 0 : Inventory.GetItemCount(foodItem.ItemId);

                if (!InCombat && foodCount == 0)
                {
                    var foodToBuy = 24 - (foodCount / stackCount);
                    //var itemsToBuy = new Dictionary<string, int>
                    //{
                    //    { container.BotSettings.Food, foodToBuy }
                    //};

                    //var currentHotspot = container.GetCurrentHotspot();
                    //if (currentHotspot.TravelPath != null)
                    //{
                    //    botTasks.Push(new TravelState(botTasks, container, currentHotspot.TravelPath.Waypoints, 0));
                    //    botTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.TravelPath.Waypoints[0]));
                    //}

                    //botTasks.Push(new BuyItemsState(botTasks, currentHotspot.Innkeeper.Name, itemsToBuy));
                    //botTasks.Push(new SellItemsState(botTasks, container, currentHotspot.Innkeeper.Name));
                    //botTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.Innkeeper.Position));
                    //container.CheckForTravelPath(botTasks, true, false);
                }

                return;
            }

            if (player.IsSpellReady(Cannibalize) && player.TastyCorpsesNearby)
            {
                player.LuaCall($"CastSpellByName('{Cannibalize}')");
                return;
            }

            if (foodItem != null && !ObjectManager.Player.IsEating && Wait.For("EatDelay", 500, true))
                foodItem.Use();
        }

        bool InCombat => ObjectManager.Aggressors.Count() > 0;
    }
}
