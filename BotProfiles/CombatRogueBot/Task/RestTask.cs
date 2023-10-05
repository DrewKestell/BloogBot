using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace CombatRogueBot
{
    class RestTask : IBotTask
    {
        const int stackCount = 5;

        const string Cannibalize = "Cannibalize";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;
        readonly WoWItem foodItem;

        public RestTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;

            //foodItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Food);
        }

        public void Update()
        {
            var readyToPop = player.HealthPercent >= 95
                || player.HealthPercent >= 80 && !player.IsEating
                || ObjectManager.Instance.Player.IsInCombat
                || ObjectManager.Instance.Units.Any(u => u.TargetGuid == ObjectManager.Instance.Player.Guid);

            if (readyToPop)
            {
                Wait.RemoveAll();
                player.Stand();
                botTasks.Pop();

                var foodCount = foodItem == null ? 0 : Inventory.Instance.GetItemCount(foodItem.Id);

                if (!InCombat && foodCount == 0)
                {
                    var foodToBuy = 28 - (foodCount / stackCount);
                    //var itemsToBuy = new Dictionary<string, int>
                    //{
                    //    { container.BotSettings.Food, foodToBuy }
                    //};

                    //var currentHotspot = container.GetCurrentHotspot();
                    //if (currentHotspot.TravelPath != null)
                    //{
                    //    botTasks.Push(new TravelState(botTasks, container, currentHotspot.TravelPath.Waypoints, 0));
                    //    botTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.TravelPath.Waypoints[0]));
                    //}

                    //botTasks.Push(new BuyItemsState(botTasks, currentHotspot.Innkeeper.Name, itemsToBuy));
                    //botTasks.Push(new SellItemsState(botTasks, container, currentHotspot.Innkeeper.Name));
                    //botTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.Innkeeper.Location));
                    //container.CheckForTravelPath(botTasks, true, false);
                }

                return;
            }

            if (player.IsChanneling)
                return;

            if (Spellbook.Instance.IsSpellReady(Cannibalize) && player.TastyCorpsesNearby)
            {
                Lua.Instance.Execute($"CastSpellByName('{Cannibalize}')");
                return;
            }

            if (foodItem != null && !ObjectManager.Instance.Player.IsEating && Wait.For("EatDelay", 500, true))
                foodItem.Use();
        }

        bool InCombat => ObjectManager.Instance.Aggressors.Count() > 0;
    }
}
