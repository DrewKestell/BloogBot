using RaidMemberBot.AI;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ElementalShamanBot
{
    class RestTask : BotTask, IBotTask
    {
        const int stackCount = 5;

        const string HealingWave = "Healing Wave";

        readonly WoWItem drinkItem;
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest)
        {
            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            if (ObjectManager.Player.TargetGuid == ObjectManager.Player.Guid)
            {
                if (Inventory.GetEquippedItems().Any(x => x.DurabilityPercentage > 0 && x.DurabilityPercentage < 100))
                {
                    Functions.LuaCall($"SendChatMessage('.repairitems')");
                }
            }
        }

        public void Update()
        {
            if (ObjectManager.Player.IsCasting) return;

            if (InCombat || (HealthOk && ManaOk))
            {
                Wait.RemoveAll();
                ObjectManager.Player.Stand();
                BotTasks.Pop();

                int drinkCount = drinkItem == null ? 0 : Inventory.GetItemCount(drinkItem.ItemId);

                if (!InCombat && drinkCount == 0)
                {
                    int drinkToBuy = 28 - (drinkCount / stackCount);
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
                    //BotTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.Innkeeper.Position));
                    //container.CheckForTravelPath(botTasks, true, false);
                }

                return;
            }

            if (!ObjectManager.Player.IsDrinking && Wait.For("HealSelfDelay", 3500, true))
            {
                ObjectManager.Player.Stand();
                if (ObjectManager.Player.HealthPercent < 70)
                    Functions.LuaCall($"CastSpellByName('{HealingWave}')");
                if (ObjectManager.Player.HealthPercent > 70 && ObjectManager.Player.HealthPercent < 85)
                {
                    if (ObjectManager.Player.Level >= 40)
                        Functions.LuaCall($"CastSpellByName('{HealingWave}(Rank 3)')");
                    else
                        Functions.LuaCall($"CastSpellByName('{HealingWave}(Rank 1)')");
                }
            }

            if (ObjectManager.Player.Level > 10 && drinkItem != null && !ObjectManager.Player.IsDrinking && ObjectManager.Player.ManaPercent < 60)
                drinkItem.Use();
        }

        bool HealthOk => ObjectManager.Player.HealthPercent > 90;

        bool ManaOk => (ObjectManager.Player.Level <= 10 && ObjectManager.Player.ManaPercent > 50) || ObjectManager.Player.ManaPercent >= 90 || (ObjectManager.Player.ManaPercent >= 65 && !ObjectManager.Player.IsDrinking);

        bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);
    }
}
