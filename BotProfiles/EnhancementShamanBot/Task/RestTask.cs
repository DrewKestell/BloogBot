using Newtonsoft.Json;
using RaidMemberBot.AI;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnhancementShamanBot
{
    class RestTask : BotTask, IBotTask
    {
        const int stackCount = 5;

        const string HealingWave = "Healing Wave";
        readonly WoWItem drinkItem;
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest) {
            List<WoWItem> drinkItems = ObjectManager.Items.Where(x => x.ItemId == 1179).ToList();
            int drinkItemsCount = drinkItems.Sum(x => x.StackCount);

            if (drinkItemsCount < 20)
            {
                Functions.LuaCall($"SendChatMessage('.additem 1179 {20 - drinkItemsCount}')");
            }

            drinkItem = ObjectManager.Items.First(x => x.ItemId == 1179);
        }

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
