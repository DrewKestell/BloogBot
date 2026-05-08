using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace FrostMageBot
{
    class ConjureItemsState : IBotState
    {
        const string ConjureFood = "Conjure Food";
        const string ConjureWater = "Conjure Water";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;
        readonly string[] configuredFoodNames;
        readonly string[] configuredDrinkNames;

        public ConjureItemsState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
            configuredFoodNames = FrostMageConsumables.GetConfiguredNames(container.BotSettings.Food);
            configuredDrinkNames = FrostMageConsumables.GetConfiguredNames(container.BotSettings.Drink);
        }

        public void Update()
        {
            var items = Inventory.GetAllItems();
            var foodSelection = FrostMageConsumables.SelectItem(
                items,
                i => i.Info?.Name,
                i => i.StackCount,
                configuredFoodNames,
                FrostMageConsumables.ConjuredFoodNames);
            var drinkSelection = FrostMageConsumables.SelectItem(
                items,
                i => i.Info?.Name,
                i => i.StackCount,
                configuredDrinkNames,
                FrostMageConsumables.ConjuredDrinkNames);

            if (player.IsCasting)
                return;

            if (player.ManaPercent < 20)
            {
                botStates.Pop();
                botStates.Push(new RestState(botStates, container));
                return;
            }

            if (Inventory.CountFreeSlots(false) == 0 || (foodSelection.HasEnough || !player.KnowsSpell(ConjureFood)) && (drinkSelection.HasEnough || !player.KnowsSpell(ConjureWater)))
            {
                botStates.Pop();

                if (player.ManaPercent <= 70)
                    botStates.Push(new RestState(botStates, container));

                return;
            }

            if (!foodSelection.HasEnough && player.IsSpellReady(ConjureFood) && Wait.For("FrostMageConjureFood", 3000, true))
            {
                player.LuaCall($"CastSpellByName('{ConjureFood}')");
                return;
            }

            if (!drinkSelection.HasEnough && player.IsSpellReady(ConjureWater) && Wait.For("FrostMageConjureDrink", 3000, true))
            {
                player.LuaCall($"CastSpellByName('{ConjureWater}')");
                return;
            }
        }
    }
}
