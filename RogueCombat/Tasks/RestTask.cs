using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace RogueCombat.Tasks
{
    internal class RestTask : BotTask, IBotTask
    {
        public RestTask(IBotContext botContext) : base(botContext)
        {
            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            if (ObjectManager.GetTarget(ObjectManager.Player).Guid == ObjectManager.Player.Guid)
            {
                if (ObjectManager.GetEquippedItems().Any(x => x.DurabilityPercentage > 0 && x.DurabilityPercentage < 100))
                {
                    ObjectManager.SendChatMessage(".repairitems");
                }
            }
        }

        public void Update()
        {
            bool readyToPop = ObjectManager.Player.HealthPercent >= 95
                || ObjectManager.Player.HealthPercent >= 80 && !ObjectManager.Player.IsEating
                || ObjectManager.Player.IsInCombat
                || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);

            if (readyToPop)
            {
                Wait.RemoveAll();
                ObjectManager.Player.DoEmote(Emote.EMOTE_STATE_STAND);
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsChanneling)
                return;

            if (ObjectManager.Player.IsSpellReady(Cannibalize) && ObjectManager.Player.TastyCorpsesNearby)
            {
                ObjectManager.Player.CastSpell(Cannibalize);
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

            if (foodItem != null && !ObjectManager.Player.IsEating && Wait.For("EatDelay", 500, true))
                foodItem.Use();
        }
    }
}
