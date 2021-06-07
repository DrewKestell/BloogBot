using BloogBot;
using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace FuryWarriorBot
{
    class RestState : IBotState
    {
        const int stackCount = 5;

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;
        readonly WoWItem foodItem;

        public RestState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;

            foodItem = Inventory.GetAllItems()
                .FirstOrDefault(i => i.Info.Name == container.BotSettings.Food);
        }

        public void Update()
        {
            if (player.HealthPercent >= 95 ||
                player.HealthPercent >= 80 && !player.IsEating ||                                                                
                ObjectManager.Player.IsInCombat ||
                ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid))
            {
                Wait.RemoveAll();
                player.Stand();
                botStates.Pop();

                var foodCount = foodItem == null ? 0 : Inventory.GetItemCount(foodItem.ItemId);
                if (!InCombat && foodCount == 0 && !container.RunningErrands)
                {
                    var foodToBuy = 28 - (foodCount / stackCount);
                    var itemsToBuy = new Dictionary<string, int>
                    {
                        { container.BotSettings.Food, foodToBuy }
                    };

                    var currentHotspot = container.GetCurrentHotspot();
                    if (currentHotspot.TravelPath != null)
                    {
                        botStates.Push(new TravelState(botStates, container, currentHotspot.TravelPath.Waypoints, 0));
                        botStates.Push(new MoveToPositionState(botStates, container, currentHotspot.TravelPath.Waypoints[0]));
                    }

                    botStates.Push(new BuyItemsState(botStates, currentHotspot.Innkeeper.Name, itemsToBuy));
                    botStates.Push(new SellItemsState(botStates, container, currentHotspot.Innkeeper.Name));
                    botStates.Push(new MoveToPositionState(botStates, container, currentHotspot.Innkeeper.Position));
                    container.CheckForTravelPath(botStates, true, false);
                    container.RunningErrands = true;
                }

                return;
            }

            if (foodItem != null && !ObjectManager.Player.IsEating && Wait.For("EatDelay", 250, true))
                foodItem.Use();
        }

        bool InCombat => ObjectManager.Aggressors.Count() > 0;
    }
}
