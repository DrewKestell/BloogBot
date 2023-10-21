using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BalanceDruidBot
{
    class RestTask : BotTask, IBotTask
    {
        const int stackCount = 5;

        const string Regrowth = "Regrowth";
        const string Rejuvenation = "Rejuvenation";
        const string MoonkinForm = "Moonkin Form";
        WoWItem drinkItem;
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest) { }

        public void Update()
        {
            if (Container.Player.IsCasting)
                return;
            
            if (InCombat)
            {
                Wait.RemoveAll();
                Container.Player.Stand();
                BotTasks.Pop();
                return;
            }
            if (HealthOk && ManaOk)
            {
                Wait.RemoveAll();
                Container.Player.Stand();
                BotTasks.Pop();

                var drinkCount = drinkItem == null ? 0 : Inventory.Instance.GetItemCount(drinkItem.Id);

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
                    //    BotTasks.Push(new TravelState(botTasks, container, currentHotspot.TravelPath.Waypoints, 0));
                    //    BotTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.TravelPath.Waypoints[0]));
                    //}

                    //BotTasks.Push(new BuyItemsState(botTasks, currentHotspot.Innkeeper.Name, itemsToBuy));
                    //BotTasks.Push(new SellItemsState(botTasks, container, currentHotspot.Innkeeper.Name));
                    //BotTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.Innkeeper.Location));
                    //container.CheckForTravelPath(botTasks, true, false);
                }
                else
                    BotTasks.Push(new BuffTask(Container, BotTasks));
            }

            if (Container.Player.HealthPercent < 60 && !Container.Player.HasBuff(Regrowth) && Wait.For("SelfHealDelay", 5000, true))
            {
                TryCastSpell(MoonkinForm, Container.Player.HasBuff(MoonkinForm));
                TryCastSpell(Regrowth);
            }
                
            if (Container.Player.HealthPercent < 80 && !Container.Player.HasBuff(Rejuvenation) && !Container.Player.HasBuff(Regrowth) && Wait.For("SelfHealDelay", 5000, true))
            {
                TryCastSpell(MoonkinForm, Container.Player.HasBuff(MoonkinForm));
                TryCastSpell(Rejuvenation);
            }

            if (Container.Player.Level >= 6 && drinkItem != null && !Container.Player.IsDrinking && Container.Player.ManaPercent < 60)
                drinkItem.Use();
        }

        bool HealthOk => Container.Player.HealthPercent >= 81;

        bool ManaOk => (Container.Player.Level < 6 && Container.Player.ManaPercent > 50) || Container.Player.ManaPercent >= 90 || (Container.Player.ManaPercent >= 65 && !Container.Player.IsDrinking);

        bool InCombat => ObjectManager.Instance.Aggressors.Count() > 0;

        void TryCastSpell(string name, bool condition = true)
        {
            if (Spellbook.Instance.IsSpellReady(name) && Container.Player.IsCasting && Container.Player.Mana > Container.Player.GetManaCost(name) && !Container.Player.IsDrinking && condition)
            {
                Container.Player.Stand();
                Lua.Instance.Execute($"CastSpellByName('{name}',1)");
            }
        }
    }
}
