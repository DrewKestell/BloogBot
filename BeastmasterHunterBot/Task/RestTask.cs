// Friday owns this file!

using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BeastMasterHunterBot
{
    // TODO: add in ammo buying/management
    class RestTask : IBotTask
    {
        const int stackCount = 5;

        const string noPetErrorMessage = "You do not have a pet";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;
        readonly LocalPet pet;
        readonly WoWItem foodItem;
        readonly WoWItem drinkItem;
        readonly WoWItem petFood;

        public RestTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Player;
            pet = ObjectManager.Pet;

            //foodItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Food);

            //drinkItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Drink);
        }

        public void Update()
        {
            // Check on your pet
            if (pet != null && !PetHappy && !PetBeingFed)
            {
                
            }
            if (player.HealthPercent >= 95 ||
                player.HealthPercent >= 80 && !player.IsEating ||
                ObjectManager.Player.IsInCombat ||
                ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid))
            {
                Wait.RemoveAll();
                player.Stand();
                botTasks.Pop();

                var foodCount = foodItem == null ? 0 : Inventory.GetItemCount(foodItem.ItemId);
                var drinkCount = drinkItem == null ? 0 : Inventory.GetItemCount(drinkItem.ItemId);

                if (!InCombat && (foodCount == 0 || drinkCount == 0))
                {
                    var foodToBuy = 12 - (foodCount / stackCount);
                    var drinkToBuy = 28 - (drinkCount / stackCount);

                    var itemsToBuy = new Dictionary<string, int>();
                    //if (foodToBuy > 0)
                    //    itemsToBuy.Add(container.BotSettings.Food, foodToBuy);
                    //if (drinkToBuy > 0)
                    //    itemsToBuy.Add(container.BotSettings.Drink, drinkToBuy);

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

            if (foodItem != null && !ObjectManager.Player.IsEating && Wait.For("EatDelay", 250, true))
                foodItem.Use();
        }

        bool InCombat => ObjectManager.Aggressors.Count() > 0;
        bool PetHealthOk => ObjectManager.Pet == null || ObjectManager.Pet.HealthPercent >= 80;
        bool PetHappy => pet.IsHappy();
        bool PetBeingFed => pet.HasBuff("Feed Pet Effect");
    }
}
