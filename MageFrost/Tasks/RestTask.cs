using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace MageFrost.Tasks
{
    internal class RestTask : BotTask, IBotTask
    {
        private const string Evocation = "Evocation";
        private readonly IWoWItem foodItem;
        private readonly IWoWItem drinkItem;
        public RestTask(IBotContext botContext) : base(botContext)
        {
            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            if (ObjectManager.GetTarget(ObjectManager.Player).Guid == ObjectManager.Player.Guid)
            {
                if (ObjectManager.GetEquippedItems().Any(x => x.DurabilityPercentage > 0 && x.DurabilityPercentage < 100))
                {
                    ObjectManager.SendChatMessage("SendChatMessage('.repairitems')");
                }
            }
        }

        public void Update()
        {
            if (ObjectManager.Player.IsChanneling)
                return;

            if (InCombat)
            {
                ObjectManager.Player.DoEmote(Emote.EMOTE_STATE_STAND);
                BotTasks.Pop();
                return;
            }

            if (HealthOk && ManaOk)
            {
                ObjectManager.Player.DoEmote(Emote.EMOTE_STATE_STAND);
                BotTasks.Pop();
                BotTasks.Push(new BuffTask(BotContext));
                return;
            }

            if (ObjectManager.Player.ManaPercent < 20 && ObjectManager.Player.IsSpellReady(Evocation))
            {
                ObjectManager.Player.CastSpell(Evocation);
                Thread.Sleep(200);
                return;
            }

            if (foodItem != null && !ObjectManager.Player.IsEating && ObjectManager.Player.HealthPercent < 80)
                foodItem.Use();

            if (drinkItem != null && !ObjectManager.Player.IsDrinking)
                drinkItem.Use();
        }

        private bool HealthOk => foodItem == null || ObjectManager.Player.HealthPercent >= 90 || (ObjectManager.Player.HealthPercent >= 80 && !ObjectManager.Player.IsEating);

        private bool ManaOk => drinkItem == null || ObjectManager.Player.ManaPercent >= 90 || (ObjectManager.Player.ManaPercent >= 80 && !ObjectManager.Player.IsDrinking);

        private bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);
    }
}
