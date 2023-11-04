// Friday owns this file!

using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BeastMasterHunterBot
{
    // TODO: add in ammo buying/management
    class RestTask : BotTask, IBotTask
    {
        const int stackCount = 5;

        const string noPetErrorMessage = "You do not have a pet";

        readonly LocalPet pet;
        readonly WoWItem foodItem;
        readonly WoWItem drinkItem;
        readonly WoWItem petFood;
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest) { }

        public void Update()
        {
            // Check on your pet
            if (pet != null && !PetHappy && !PetBeingFed)
            {
                
            }
            if (Container.Player.HealthPercent >= 95 ||
                Container.Player.HealthPercent >= 80 && !Container.Player.IsEating ||
                ObjectManager.Instance.Player.IsInCombat ||
                ObjectManager.Instance.Units.Any(u => u.TargetGuid == ObjectManager.Instance.Player.Guid))
            {
                Wait.RemoveAll();
                Container.Player.Stand();
                BotTasks.Pop();

                int foodCount = foodItem == null ? 0 : Inventory.Instance.GetItemCount(foodItem.Id);
                int drinkCount = drinkItem == null ? 0 : Inventory.Instance.GetItemCount(drinkItem.Id);

                if (!InCombat && (foodCount == 0 || drinkCount == 0))
                {
                    int foodToBuy = 12 - (foodCount / stackCount);
                    int drinkToBuy = 28 - (drinkCount / stackCount);

                    Dictionary<string, int> itemsToBuy = new Dictionary<string, int>();
                    //if (foodToBuy > 0)
                    //    itemsToBuy.Add(container.BotSettings.Food, foodToBuy);
                    //if (drinkToBuy > 0)
                    //    itemsToBuy.Add(container.BotSettings.Drink, drinkToBuy);

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

            if (foodItem != null && !ObjectManager.Instance.Player.IsEating && Wait.For("EatDelay", 250, true))
                foodItem.Use();
        }

        bool InCombat => ObjectManager.Instance.Aggressors.Count() > 0;
        bool PetHealthOk => ObjectManager.Instance.Pet == null || ObjectManager.Instance.Pet.HealthPercent >= 80;
        bool PetHappy => pet.IsHappy();
        bool PetBeingFed => pet.HasBuff("Feed Pet Effect");
    }
}
