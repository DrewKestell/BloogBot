using RaidMemberBot.AI;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ProtectionWarriorBot
{
    class RestTask : BotTask, IBotTask
    {
        const int stackCount = 5;

        readonly WoWItem foodItem;
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest)
        {
            List<WoWItem> foodItems = ObjectManager.Items.Where(x => x.ItemId == 5479).ToList();
            int foodItemsCount = foodItems.Sum(x => x.StackCount);

            if (foodItemsCount < 20)
            {
                Functions.LuaCall($"SendChatMessage('.additem 5479 {20 - foodItemsCount}')");
            }

            foodItem = ObjectManager.Items.First(x => x.ItemId == 5479);
        }

        public void Update()
        {
            if (ObjectManager.Player.HealthPercent >= 95 ||
                ObjectManager.Player.HealthPercent >= 80 && !ObjectManager.Player.IsEating ||
                ObjectManager.Player.IsInCombat ||
                ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid))
            {
                ObjectManager.Player.Stand();
                BotTasks.Pop();
                return;
            }

            if (foodItem != null && !ObjectManager.Player.IsEating)
                foodItem.Use();
        }

        bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);
    }
}
