using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace WarriorProtection.Tasks
{
    internal class RestTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            if (ObjectManager.Player.HealthPercent >= 95 ||
                ObjectManager.Player.HealthPercent >= 80 && !ObjectManager.Player.IsEating ||
                ObjectManager.Player.IsInCombat ||
                ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid))
            {
                ObjectManager.Player.DoEmote(Emote.EMOTE_STATE_STAND);
                BotTasks.Pop();
                return;
            }

            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            if (ObjectManager.GetTarget(ObjectManager.Player).Guid == ObjectManager.Player.Guid)
            {
                if (ObjectManager.GetEquippedItems().Any(x => x.DurabilityPercentage > 0 && x.DurabilityPercentage < 100))
                {
                    ObjectManager.SendChatMessage(".repairitems");
                }

                List<IWoWItem> foodItems = ObjectManager.Items.Where(x => x.ItemId == 5479).ToList();
                uint foodItemsCount = (uint)foodItems.Sum(x => x.StackCount);

                if (foodItemsCount < 20)
                {
                    ObjectManager.SendChatMessage($".additem 5479 {20 - foodItemsCount}");
                }
            }

            IWoWItem foodItem = ObjectManager.Items.First(x => x.ItemId == 5479);

            if (foodItem != null && !ObjectManager.Player.IsEating)
                foodItem.Use();
        }
    }
}
