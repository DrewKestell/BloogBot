using BloogBot;
using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace RetributionPaladinBot
{
    class RestState : IBotState
    {
        const int stackCount = 5;

        const string HolyLight = "Holy Light";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;
        readonly WoWItem drinkItem;
        
        public RestState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
            player.SetTarget(player.Guid);

            drinkItem = Inventory.GetAllItems()
                .FirstOrDefault(i => i.Info.Name == container.BotSettings.Drink);
        }

        public void Update()
        {
            if (player.IsCasting) return;

            if (InCombat || (HealthOk && ManaOk))
            {
                Wait.RemoveAll();
                player.Stand();
                botStates.Pop();

                var drinkCount = drinkItem == null ? 0 : Inventory.GetItemCount(drinkItem.ItemId);
                if (!InCombat && drinkCount == 0 && !container.RunningErrands)
                {
                    var drinkToBuy = 28 - (drinkCount / stackCount);
                    var itemsToBuy = new Dictionary<string, int>
                    {
                        { container.BotSettings.Drink, drinkToBuy }
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
                else
                    botStates.Push(new BuffSelfState(botStates, container));

            }
            
            if (!player.IsDrinking && Wait.For("HealSelfDelay", 3500, true))
            {
                player.Stand();
                if (player.HealthPercent < 70)
                    player.LuaCall($"CastSpellByName('{HolyLight}')");
                if (player.HealthPercent > 70 && player.HealthPercent < 90)
                    player.LuaCall($"CastSpellByName('{HolyLight}(Rank 1)')");
            }

            if (player.Level > 10 && drinkItem != null && !player.IsDrinking && player.ManaPercent < 60 && Wait.For("UseDrinkDelay", 1000, true))
                drinkItem.Use();
        }

        bool HealthOk => player.HealthPercent > 90;

        bool ManaOk => (player.Level <= 10 && player.ManaPercent > 50) || player.ManaPercent >= 90 || (player.ManaPercent >= 65 && !player.IsDrinking);

        bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);
    }
}
