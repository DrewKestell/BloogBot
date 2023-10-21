using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ProtectionPaladinBot
{
    class RestTask : BotTask, IBotTask
    {
        const int stackCount = 5;

        const string HolyLight = "Holy Light";

        readonly WoWItem drinkItem;

        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest) { }

        public void Update()
        {
            if (Container.Player.IsCasting) return;

            if (InCombat || (HealthOk && ManaOk))
            {
                Wait.RemoveAll();
                Container.Player.Stand();
                BotTasks.Pop();

                var drinkCount = drinkItem == null ? 0 : Inventory.Instance.GetItemCount(drinkItem.Id);
                if (!InCombat && drinkCount == 0)
                {
                    var drinkToBuy = 28 - (drinkCount / stackCount);
                    //var itemsToBuy = new Dictionary<string, int>
                    //{
                    //    { container.BotSettings.Drink, drinkToBuy }
                    //};

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
            
            if (!Container.Player.IsDrinking && Wait.For("HealSelfDelay", 3500, true))
            {
                Container.Player.Stand();
                if (Container.Player.HealthPercent < 70)
                    Lua.Instance.Execute($"CastSpellByName('{HolyLight}')");
                if (Container.Player.HealthPercent > 70 && Container.Player.HealthPercent < 90)
                    Lua.Instance.Execute($"CastSpellByName('{HolyLight}(Rank 1)')");
            }

            if (Container.Player.Level > 10 && drinkItem != null && !Container.Player.IsDrinking && Container.Player.ManaPercent < 60 && Wait.For("UseDrinkDelay", 1000, true))
                drinkItem.Use();
        }

        bool HealthOk => Container.Player.HealthPercent > 90;

        bool ManaOk => (Container.Player.Level <= 10 && Container.Player.ManaPercent > 50) || Container.Player.ManaPercent >= 90 || (Container.Player.ManaPercent >= 65 && !Container.Player.IsDrinking);

        bool InCombat => ObjectManager.Instance.Player.IsInCombat || ObjectManager.Instance.Units.Any(u => u.TargetGuid == ObjectManager.Instance.Player.Guid);
    }
}
