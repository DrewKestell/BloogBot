using BloogBot;
using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AfflictionWarlockBot
{
    class RestState : IBotState
    {
        const int stackCount = 5;

        const string ConsumeShadows = "Consume Shadows";
        const string HealthFunnel = "Health Funnel";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;
        
        readonly WoWItem foodItem;
        readonly WoWItem drinkItem;
        LocalPet pet;

        public RestState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
            player.SetTarget(player.Guid);

            foodItem = Inventory.GetAllItems()
                .FirstOrDefault(i => i.Info.Name == container.BotSettings.Food);

            drinkItem = Inventory.GetAllItems()
                .FirstOrDefault(i => i.Info.Name == container.BotSettings.Drink);
        }

        public void Update()
        {
            pet = ObjectManager.Pet;

            if (pet != null && pet.HealthPercent < 60 && pet.CanUse(ConsumeShadows) && !pet.IsCasting && !pet.IsChanneling)
                pet.Cast(ConsumeShadows);

            if (InCombat || (HealthOk && ManaOk))
            {
                if (!player.IsCasting && !player.IsChanneling)
                    player.Stand();

                if (InCombat || PetHealthOk)
                {
                    pet?.FollowPlayer();
                    botStates.Pop();

                    var foodCount = foodItem == null ? 0 : Inventory.GetItemCount(foodItem.ItemId);
                    var drinkCount = drinkItem == null ? 0 : Inventory.GetItemCount(drinkItem.ItemId);

                    if (!InCombat && (foodCount == 0 || drinkCount == 0) && !container.RunningErrands)
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
                    else
                        botStates.Push(new SummonVoidwalkerState(botStates));
                }
                else
                {
                    if (!player.IsChanneling && !player.IsCasting && player.KnowsSpell(HealthFunnel) && player.HealthPercent > 30)
                        player.LuaCall($"CastSpellByName('{HealthFunnel}')");
                }

                return;
            }

            if (foodItem != null && !player.IsEating && player.HealthPercent < 80 && Wait.For("EatDelay", 500, true))
                foodItem.Use();

            if (drinkItem != null && !player.IsDrinking && player.ManaPercent < 60 && Wait.For("DrinkDelay", 500, true))
                drinkItem.Use();
        }

        bool HealthOk => foodItem == null || player.HealthPercent >= 90 || (player.HealthPercent >= 70 && !player.IsEating);

        bool PetHealthOk => ObjectManager.Pet == null || ObjectManager.Pet.HealthPercent >= 80;

        bool ManaOk => (player.Level < 6 && player.ManaPercent > 50) || player.ManaPercent >= 90 || (player.ManaPercent >= 55 && !player.IsDrinking);

        bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid || u.TargetGuid == ObjectManager.Pet?.Guid);
    }
}
