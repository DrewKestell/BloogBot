using RaidMemberBot.AI;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ProtectionPaladinBot
{
    class RestTask : BotTask, IBotTask
    {
        const int stackCount = 5;

        const string HolyLight = "Holy Light";

        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest) { }

        public void Update()
        {
            if (ObjectManager.Player.IsCasting) return;

            if (InCombat || (HealthOk && ManaOk))
            {
                Wait.RemoveAll();
                ObjectManager.Player.Stand();
                BotTasks.Pop();
                return;

            }
            
            if (!ObjectManager.Player.IsDrinking && Wait.For("HealSelfDelay", 3500, true))
            {
                ObjectManager.Player.Stand();
                if (ObjectManager.Player.HealthPercent < 70)
                    Functions.LuaCall($"CastSpellByName('{HolyLight}')");
                if (ObjectManager.Player.HealthPercent > 70 && ObjectManager.Player.HealthPercent < 90)
                    Functions.LuaCall($"CastSpellByName('{HolyLight}(Rank 1)')");
            }

            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            if (ObjectManager.Player.TargetGuid == ObjectManager.Player.Guid)
            {
                if (Inventory.GetEquippedItems().Any(x => x.DurabilityPercentage > 0 && x.DurabilityPercentage < 100))
                {
                    Functions.LuaCall($"SendChatMessage('.repairitems')");
                }

                List<WoWItem> foodItems = ObjectManager.Items.Where(x => x.ItemId == 5479).ToList();
                int foodItemsCount = foodItems.Sum(x => x.StackCount);

                if (foodItemsCount < 20)
                {
                    Functions.LuaCall($"SendChatMessage('.additem 5479 {20 - foodItemsCount}')");
                }

                List<WoWItem> drinkItems = ObjectManager.Items.Where(x => x.ItemId == 1179).ToList();
                int drinkItemsCount = drinkItems.Sum(x => x.StackCount);

                if (drinkItemsCount < 20)
                {
                    Functions.LuaCall($"SendChatMessage('.additem 1179 {20 - drinkItemsCount}')");
                }
            }

            WoWItem foodItem = ObjectManager.Items.First(x => x.ItemId == 5479);
            WoWItem drinkItem = ObjectManager.Items.First(x => x.ItemId == 1179);

            if (ObjectManager.Player.Level > 10 && drinkItem != null && !ObjectManager.Player.IsDrinking && ObjectManager.Player.ManaPercent < 60 && Wait.For("UseDrinkDelay", 1000, true))
                drinkItem.Use();
        }

        bool HealthOk => ObjectManager.Player.HealthPercent > 90;

        bool ManaOk => (ObjectManager.Player.Level <= 10 && ObjectManager.Player.ManaPercent > 50) || ObjectManager.Player.ManaPercent >= 90 || (ObjectManager.Player.ManaPercent >= 65 && !ObjectManager.Player.IsDrinking);

        bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);
    }
}
