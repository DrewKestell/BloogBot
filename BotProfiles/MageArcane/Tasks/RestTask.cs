using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace MageArcane.Tasks
{
    internal class RestTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        private const string Evocation = "Evocation";
        private readonly IWoWItem foodItem;
        private readonly IWoWItem drinkItem;

        public void Update()
        {
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

            if (ObjectManager.Player.IsChanneling)
                return;

            if (ObjectManager.Player.ManaPercent < 20 && ObjectManager.Player.IsSpellReady(Evocation))
            {
                ObjectManager.Player.CastSpell(Evocation);
                return;
            }

            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            if (ObjectManager.GetTarget(ObjectManager.Player).Guid == ObjectManager.Player.Guid)
            {
                if (ObjectManager.GetEquippedItems().Any(x => x.DurabilityPercentage > 0 && x.DurabilityPercentage < 100))
                {
                    ObjectManager.SendChatMessage("SendChatMessage('.repairitems')");
                }
            }

            if (ObjectManager.Player.Level > 3 && foodItem != null && !ObjectManager.Player.IsEating && ObjectManager.Player.HealthPercent < 80)
                foodItem.Use();

            if (ObjectManager.Player.Level > 3 && drinkItem != null && !ObjectManager.Player.IsDrinking && ObjectManager.Player.ManaPercent < 80)
                drinkItem.Use();
        }

        private bool HealthOk => ObjectManager.Player.HealthPercent > 90;

        private bool ManaOk => (ObjectManager.Player.Level < 6 && ObjectManager.Player.ManaPercent > 60) || ObjectManager.Player.ManaPercent >= 90 || (ObjectManager.Player.ManaPercent >= 75 && !ObjectManager.Player.IsDrinking);

        private bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);
    }
}
