using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FrostMageBot
{
    class RestState : IBotState
    {
        const string Evocation = "Evocation";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;
        readonly string[] configuredFoodNames;
        readonly string[] configuredDrinkNames;

        WoWItem foodItem;
        WoWItem drinkItem;

        public RestState(Stack<IBotState> botStates, IDependencyContainer container)
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
            foodItem = FrostMageConsumables.SelectItem(
                items,
                i => i.Info?.Name,
                i => i.StackCount,
                configuredFoodNames,
                FrostMageConsumables.ConjuredFoodNames).Item;
            drinkItem = FrostMageConsumables.SelectItem(
                items,
                i => i.Info?.Name,
                i => i.StackCount,
                configuredDrinkNames,
                FrostMageConsumables.ConjuredDrinkNames).Item;

            if (player.IsChanneling)
                return;

            if (InCombat)
            {
                player.Stand();
                botStates.Pop();
                return;
            }

            if (HealthOk && ManaOk)
            {
                player.Stand();
                botStates.Pop();
                botStates.Push(new BuffSelfState(botStates, container));
                return;
            }

            if (player.ManaPercent < 20 && player.IsSpellReady(Evocation))
            {
                player.LuaCall($"CastSpellByName('{Evocation}')");
                Thread.Sleep(200);
                return;
            }

            if (foodItem != null && !player.IsEating && player.HealthPercent < 80)
                foodItem.Use();

            if (drinkItem != null && !player.IsDrinking)
                drinkItem.Use();
        }

        bool HealthOk => foodItem == null || player.HealthPercent >= 90 || (player.HealthPercent >= 80 && !player.IsEating);

        bool ManaOk => drinkItem == null || player.ManaPercent >= 90 || (player.ManaPercent >= 80 && !player.IsDrinking);

        bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);
    }
}
