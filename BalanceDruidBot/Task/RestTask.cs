using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BalanceDruidBot
{
    class RestTask : IBotTask
    {
        const int stackCount = 5;

        const string Regrowth = "Regrowth";
        const string Rejuvenation = "Rejuvenation";
        const string MoonkinForm = "Moonkin Form";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;
        readonly WoWItem drinkItem;

        public RestTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Player;

            //drinkItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Drink);
        }

        public void Update()
        {
            if (player.IsCasting)
                return;
            
            if (InCombat)
            {
                Wait.RemoveAll();
                player.Stand();
                botTasks.Pop();
                return;
            }
            if (HealthOk && ManaOk)
            {
                Wait.RemoveAll();
                player.Stand();
                botTasks.Pop();

                var drinkCount = drinkItem == null ? 0 : Inventory.GetItemCount(drinkItem.ItemId);

                if (!InCombat && drinkCount == 0)
                {
                    var drinkToBuy = 28 - (drinkCount / stackCount);
                    //var itemsToBuy = new Dictionary<string, int>
                    //    {
                    //        { container.BotSettings.Drink, drinkToBuy }
                    //    };

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
                else
                    botTasks.Push(new BuffTask(container, botTasks, new List<WoWUnit>() { ObjectManager.Player }));
            }

            if (player.HealthPercent < 60 && !player.HasBuff(Regrowth) && Wait.For("SelfHealDelay", 5000, true))
            {
                TryCastSpell(MoonkinForm, player.HasBuff(MoonkinForm));
                TryCastSpell(Regrowth);
            }
                
            if (player.HealthPercent < 80 && !player.HasBuff(Rejuvenation) && !player.HasBuff(Regrowth) && Wait.For("SelfHealDelay", 5000, true))
            {
                TryCastSpell(MoonkinForm, player.HasBuff(MoonkinForm));
                TryCastSpell(Rejuvenation);
            }

            if (player.Level >= 6 && drinkItem != null && !player.IsDrinking && player.ManaPercent < 60)
                drinkItem.Use();
        }

        bool HealthOk => player.HealthPercent >= 81;

        bool ManaOk => (player.Level < 6 && player.ManaPercent > 50) || player.ManaPercent >= 90 || (player.ManaPercent >= 65 && !player.IsDrinking);

        bool InCombat => ObjectManager.Aggressors.Count() > 0;

        void TryCastSpell(string name, bool condition = true)
        {
            if (player.IsSpellReady(name) && !player.IsCasting && player.Mana > player.GetManaCost(name) && !player.IsDrinking && condition)
            {
                player.Stand();
                player.LuaCall($"CastSpellByName('{name}',1)");
            }
        }
    }
}
