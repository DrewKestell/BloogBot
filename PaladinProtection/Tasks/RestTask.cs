using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace PaladinProtection.Tasks
{
    internal class RestTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        private const int stackCount = 5;
        private const string HolyLight = "Holy Light";

        public void Update()
        {
            if (ObjectManager.Player.IsCasting) return;

            if (InCombat || (HealthOk && ManaOk))
            {
                Wait.RemoveAll();
                ObjectManager.Player.DoEmote(Emote.EMOTE_STATE_STAND);
                BotTasks.Pop();
                return;

            }
            
            if (!ObjectManager.Player.IsDrinking && Wait.For("HealSelfDelay", 3500, true))
            {
                ObjectManager.Player.DoEmote(Emote.EMOTE_STATE_STAND);
                if (ObjectManager.Player.HealthPercent < 70)
                    ObjectManager.Player.CastSpell(HolyLight);
                if (ObjectManager.Player.HealthPercent > 70 && ObjectManager.Player.HealthPercent < 90)
                    ObjectManager.Player.CastSpell(HolyLight, 1);
            }

            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            if (ObjectManager.GetTarget(ObjectManager.Player).Guid == ObjectManager.Player.Guid)
            {
                if (ObjectManager.GetEquippedItems().Any(x => x.DurabilityPercentage > 0 && x.DurabilityPercentage < 100))
                {
                    ObjectManager.SendChatMessage("SendChatMessage('.repairitems')");
                }

                List<IWoWItem> foodItems = ObjectManager.Items.Where(x => x.ItemId == 5479).ToList();
                uint foodItemsCount = (uint)foodItems.Sum(x => x.StackCount);

                if (foodItemsCount < 20)
                {
                    ObjectManager.SendChatMessage("SendChatMessage('.additem 5479 " + (20 - foodItemsCount) + "')");
                }

                List<IWoWItem> drinkItems = ObjectManager.Items.Where(x => x.ItemId == 1179).ToList();
                uint drinkItemsCount = (uint)drinkItems.Sum(x => x.StackCount);

                if (drinkItemsCount < 20)
                {
                    ObjectManager.SendChatMessage("SendChatMessage('.additem 1179 " + (20 - drinkItemsCount) + "')");
                }
            }

            IWoWItem foodItem = ObjectManager.Items.First(x => x.ItemId == 5479);
            IWoWItem drinkItem = ObjectManager.Items.First(x => x.ItemId == 1179);

            if (ObjectManager.Player.Level > 10 && drinkItem != null && !ObjectManager.Player.IsDrinking && ObjectManager.Player.ManaPercent < 60 && Wait.For("UseDrinkDelay", 1000, true))
                drinkItem.Use();
        }

        private bool HealthOk => ObjectManager.Player.HealthPercent > 90;

        private bool ManaOk => (ObjectManager.Player.Level <= 10 && ObjectManager.Player.ManaPercent > 50) || ObjectManager.Player.ManaPercent >= 90 || (ObjectManager.Player.ManaPercent >= 65 && !ObjectManager.Player.IsDrinking);

        private bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);
    }
}
