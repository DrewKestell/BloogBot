using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace PriestShadow.Tasks
{
    internal class RestTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {

        public void Update()
        {
            if (ObjectManager.Player.IsCasting) return;

            if (InCombat || (HealthOk && ManaOk))
            {
                if (ObjectManager.Player.IsSpellReady(ShadowForm) && !ObjectManager.Player.HasBuff(ShadowForm) && ObjectManager.Player.IsDiseased)
                {
                    if (ObjectManager.Player.IsSpellReady(AbolishDisease))
                        ObjectManager.Player.CastSpell(AbolishDisease);
                    else if (ObjectManager.Player.IsSpellReady(CureDisease))
                        ObjectManager.Player.CastSpell(CureDisease);

                    return;
                }

                if (ObjectManager.Player.IsSpellReady(ShadowForm) && !ObjectManager.Player.HasBuff(ShadowForm))
                    ObjectManager.Player.CastSpell(ShadowForm);

                Wait.RemoveAll();
                ObjectManager.Player.DoEmote(Emote.EMOTE_STATE_STAND);
                BotTasks.Pop();
                    BotTasks.Push(new BuffTask(BotContext));

                return;
            }
            else
                ObjectManager.Player.StopAllMovement();

            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            if (ObjectManager.GetTarget(ObjectManager.Player) == null) return;

            if (ObjectManager.GetTarget(ObjectManager.Player).Guid == ObjectManager.Player.Guid)
            {
                if (ObjectManager.GetEquippedItems().Any(x => x.DurabilityPercentage > 0 && x.DurabilityPercentage < 100))
                {
                    ObjectManager.SendChatMessage("SendChatMessage('.repairitems')");
                }

                List<IWoWItem> drinkItems = ObjectManager.Items.Where(x => x.ItemId == 1179).ToList();
                uint drinkItemsCount = (uint)drinkItems.Sum(x => x.StackCount);

                if (drinkItemsCount < 20)
                {
                    ObjectManager.SendChatMessage($".additem 1179 {20 - drinkItemsCount}");
                }

                IWoWItem drinkItem = ObjectManager.Items.First(x => x.ItemId == 1179);

                if (ObjectManager.Player.Level >= 5 && drinkItem != null && !ObjectManager.Player.IsDrinking && ObjectManager.Player.ManaPercent < 60)
                {
                    drinkItem.Use();
                }
            }

            if (!ObjectManager.Player.IsDrinking && Wait.For("HealSelfDelay", 3500, true))
            {
                ObjectManager.Player.DoEmote(Emote.EMOTE_STATE_STAND);

                if (ObjectManager.Player.HealthPercent < 70)
                {
                    if (ObjectManager.Player.HasBuff(ShadowForm))
                        ObjectManager.Player.CastSpell(ShadowForm);
                }

                if (ObjectManager.Player.HealthPercent < 50)
                {
                    if (ObjectManager.Player.IsSpellReady(Heal))
                        ObjectManager.Player.CastSpell(Heal);
                    else
                        ObjectManager.Player.CastSpell(LesserHeal);
                }

                if (ObjectManager.Player.HealthPercent < 70)
                    ObjectManager.Player.CastSpell(LesserHeal, castOnSelf: true);
            }
        }

        private bool HealthOk => ObjectManager.Player.HealthPercent > 90;

        private bool ManaOk => (ObjectManager.Player.Level < 5 && ObjectManager.Player.ManaPercent > 50) || ObjectManager.Player.ManaPercent >= 90 || (ObjectManager.Player.ManaPercent >= 65 && !ObjectManager.Player.IsDrinking);

        private bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);
    }
}
