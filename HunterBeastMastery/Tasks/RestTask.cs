using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace HunterBeastMastery.Tasks
{
    // TODO: add in ammo buying/management
    internal class RestTask : BotTask, IBotTask
    {
        private const int stackCount = 5;
        private const string noPetErrorMessage = "You do not have a pet";
        private readonly ILocalPet pet;
        private readonly IWoWItem foodItem;
        private readonly IWoWItem drinkItem;
        private readonly IWoWItem petFood;
        public RestTask(IBotContext botContext) : base(botContext)
        {
            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            if (ObjectManager.Player.TargetGuid == ObjectManager.Player.Guid)
            {
                if (ObjectManager.GetEquippedItems().Any(x => x.DurabilityPercentage > 0 && x.DurabilityPercentage < 100))
                {
                    ObjectManager.SendChatMessage("SendChatMessage('.repairitems')");
                }
            }
        }

        public void Update()
        {
            // Check on your pet
            if (pet != null && !PetHappy && !PetBeingFed)
            {
                
            }
            if (ObjectManager.Player.HealthPercent >= 95 ||
                ObjectManager.Player.HealthPercent >= 80 && !ObjectManager.Player.IsEating ||
                ObjectManager.Player.IsInCombat ||
                ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid))
            {
                Wait.RemoveAll();
                ObjectManager.Player.DoEmote(Emote.EMOTE_STATE_STAND);
                BotTasks.Pop();

                uint foodCount = foodItem == null ? 0 : ObjectManager.GetItemCount(foodItem.ItemId);
                uint drinkCount = drinkItem == null ? 0 : ObjectManager.GetItemCount(drinkItem.ItemId);

                if (!InCombat && (foodCount == 0 || drinkCount == 0))
                {
                    uint foodToBuy = 12 - (foodCount / stackCount);
                    uint drinkToBuy = 28 - (drinkCount / stackCount);

                    Dictionary<string, uint> itemsToBuy = [];
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
                    //BotTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.Innkeeper.Position));
                    //container.CheckForTravelPath(botTasks, true, false);
                }

                return;
            }

            if (foodItem != null && !ObjectManager.Player.IsEating && Wait.For("EatDelay", 250, true))
                foodItem.Use();
        }

        private bool InCombat => ObjectManager.Aggressors.Any();

        private bool PetHealthOk => ObjectManager.Pet == null || ObjectManager.Pet.HealthPercent >= 80;

        private bool PetHappy => pet.IsHappy();

        private bool PetBeingFed => pet.HasBuff("Feed Pet Effect");
    }
}
