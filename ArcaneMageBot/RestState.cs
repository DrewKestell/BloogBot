using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ArcaneMageBot
{
    class RestState : IBotState
    {
        const string Evocation = "Evocation";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;

        readonly WoWItem foodItem;
        readonly WoWItem drinkItem;
        
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

            if (player.IsChanneling)
                return;

            if (player.ManaPercent < 20 && player.IsSpellReady(Evocation))
            {
                player.LuaCall($"CastSpellByName('{Evocation}')");
                return;
            }

            if (player.Level > 3 && foodItem != null && !player.IsEating && player.HealthPercent < 80)
                foodItem.Use();

            if (player.Level > 3 && drinkItem != null && !player.IsDrinking && player.ManaPercent < 80)
                drinkItem.Use();
        }

        bool HealthOk => player.HealthPercent > 90;

        bool ManaOk => (player.Level < 6 && player.ManaPercent > 60) || player.ManaPercent >= 90 || (player.ManaPercent >= 75 && !player.IsDrinking);

        bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);
    }
}
