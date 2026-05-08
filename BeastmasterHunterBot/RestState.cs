// Friday owns this file!

using BloogBot;
using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BeastMasterHunterBot
{
    // TODO: add in ammo buying/management
    class RestState : IBotState
    {
        const int stackCount = 5;

        const string MendPet = "Mend Pet";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;
        WoWItem foodItem;
        WoWItem drinkItem;

        public RestState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;

            foodItem = Inventory.GetAllItems()
                .FirstOrDefault(i => i.Info.Name == container.BotSettings.Food);

            drinkItem = Inventory.GetAllItems()
                .FirstOrDefault(i => i.Info.Name == container.BotSettings.Drink);
        }

        public void Update()
        {
            if (InCombat)
            {
                Wait.RemoveAll();
                player.Stand();
                botStates.Pop();
                return;
            }

            var pet = ObjectManager.Pet;
            if (pet != null && !pet.IsHappy() && !pet.HasBuff("Feed Pet Effect") && Wait.For("FeedPetDelay", 3000, true))
            {
                FeedPet();
            }

            if (foodItem != null && !player.IsEating && player.HealthPercent < 80 && Wait.For("EatDelay", 2000, true))
                foodItem.Use();

            if (drinkItem != null && !player.IsDrinking && player.ManaPercent < 80 && Wait.For("DrinkDelay", 2000, true))
                drinkItem.Use();

            if (pet != null && pet.HealthPercent > 0 && pet.HealthPercent < 90
                && !pet.HasBuff(MendPet) && player.IsSpellReady(MendPet))
            {
                player.LuaCall($"CastSpellByName('{MendPet}')");
            }

            if (HealthOk && ManaOk && PetHealthOk)
            {
                Wait.RemoveAll();
                player.Stand();
                botStates.Pop();
                botStates.Push(new BuffSelfState(botStates, container));

                var foodCount = foodItem == null ? 0 : Inventory.GetItemCount(foodItem.ItemId);
                var drinkCount = drinkItem == null ? 0 : Inventory.GetItemCount(drinkItem.ItemId);

                if (foodCount == 0 || drinkCount == 0)
                {
                    var foodToBuy = 12 - (foodCount / stackCount);
                    var drinkToBuy = 28 - (drinkCount / stackCount);

                    var itemsToBuy = new Dictionary<string, int>();
                    if (foodToBuy > 0)
                        itemsToBuy.Add(container.BotSettings.Food, foodToBuy);
                    if (drinkToBuy > 0)
                        itemsToBuy.Add(container.BotSettings.Drink, drinkToBuy);

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
        }

        void FeedPet()
        {
            if (string.IsNullOrEmpty(container.BotSettings.Food)) return;

            var foodName = container.BotSettings.Food;

            player.LuaCall("CastSpellByName('Feed Pet')");
            player.LuaCall(
                "for bag = 0,4 do for slot = 1,GetContainerNumSlots(bag) do local item = GetContainerItemLink(bag,slot) " +
                "if item then if string.find(item, '" + foodName.Replace("'", "\\'") + "') then " +
                "PickupContainerItem(bag,slot) break end end end end");
            player.LuaCall("ClearCursor()");
        }

        bool HealthOk => foodItem == null || player.HealthPercent >= 95 || (player.HealthPercent >= 80 && !player.IsEating);

        bool ManaOk => drinkItem == null || player.ManaPercent >= 95 || (player.ManaPercent >= 80 && !player.IsDrinking);

        bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);

        bool PetHealthOk
        {
            get
            {
                var pet = ObjectManager.Pet;
                return pet == null || pet.HealthPercent == 0 || pet.HealthPercent >= 90;
            }
        }
    }
}
