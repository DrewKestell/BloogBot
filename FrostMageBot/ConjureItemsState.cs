using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace FrostMageBot
{
    class ConjureItemsState : IBotState
    {
        const string ConjureFood = "Conjure Food";
        const string ConjureWater = "Conjure Water";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;

        WoWItem foodItem;
        WoWItem drinkItem;

        public ConjureItemsState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            foodItem = Inventory.GetAllItems()
                .FirstOrDefault(i => i.Info.Name == container.BotSettings.Food);

            drinkItem = Inventory.GetAllItems()
                .FirstOrDefault(i => i.Info.Name == container.BotSettings.Drink);

            if (player.IsCasting)
                return;

            //player.Stand();

            if (player.ManaPercent < 20)
            {
                botStates.Pop();
                botStates.Push(new RestState(botStates, container));
                return;
            }

            if (Inventory.CountFreeSlots(false) == 0 || (foodItem != null || !player.KnowsSpell(ConjureFood)) && (drinkItem != null || !player.KnowsSpell(ConjureWater)))
            {
                botStates.Pop();

                if (player.ManaPercent <= 70)
                    botStates.Push(new RestState(botStates, container));

                return;
            }

            var foodCount = foodItem == null ? 0 : Inventory.GetItemCount(foodItem.ItemId);
            if (foodItem == null || foodCount <= 2)
                TryCastSpell(ConjureFood);

            var drinkCount = drinkItem == null ? 0 : Inventory.GetItemCount(drinkItem.ItemId);
            if (drinkItem == null || drinkCount <= 2)
                TryCastSpell(ConjureWater);
        }

        void TryCastSpell(string name)
        {
            if (player.IsSpellReady(name) && !player.IsCasting)
                player.LuaCall($"CastSpellByName('{name}')");
        }
    }
}
