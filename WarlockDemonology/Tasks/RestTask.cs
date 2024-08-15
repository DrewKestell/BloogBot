using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace WarlockDemonology.Tasks
{
    public class RestTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            if (ObjectManager.Pet != null && ObjectManager.Pet.HealthPercent < 60 && ObjectManager.Pet.CanUse(ConsumeShadows) && ObjectManager.Pet.IsCasting && ObjectManager.Pet.ChannelingId == 0)
                ObjectManager.Pet.Cast(ConsumeShadows);

            if (InCombat || (HealthOk && ManaOk))
            {
                if (ObjectManager.Player.IsCasting && ObjectManager.Player.ChannelingId == 0)
                    ObjectManager.Player.DoEmote(Emote.EMOTE_STATE_STAND);

                if (InCombat || PetHealthOk)
                {
                    ObjectManager.Pet?.FollowPlayer();
                    BotTasks.Pop();

                    BotTasks.Push(new SummonPetTask(BotContext));
                }
                else
                {
                    if (ObjectManager.Player.ChannelingId == 0 && ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(HealthFunnel) && ObjectManager.Player.HealthPercent > 30)
                        ObjectManager.Player.CastSpell(HealthFunnel);
                }

                return;
            }
            else
                ObjectManager.Player.StopAllMovement();

            TryCastSpell(LifeTap, 0, int.MaxValue, ObjectManager.Player.HealthPercent > 85 && ObjectManager.Player.ManaPercent < 80);

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
                    ObjectManager.SendChatMessage($".additem 5479 {20 - foodItemsCount}");

                IWoWItem foodItem = ObjectManager.Items.First(x => x.ItemId == 5479);

                List<IWoWItem> drinkItems = ObjectManager.Items.Where(x => x.ItemId == 1179).ToList();
                uint drinkItemsCount = (uint)drinkItems.Sum(x => x.StackCount);

                if (drinkItemsCount < 20)
                    ObjectManager.SendChatMessage($".additem 1179 {20 - drinkItemsCount}");

                IWoWItem drinkItem = ObjectManager.Items.First(x => x.ItemId == 1179);

                if (foodItem != null && !ObjectManager.Player.IsEating && ObjectManager.Player.HealthPercent < 80 && Wait.For("EatDelay", 500, true))
                    foodItem.Use();

                if (drinkItem != null && !ObjectManager.Player.IsDrinking && ObjectManager.Player.ManaPercent < 60 && Wait.For("DrinkDelay", 500, true))
                    drinkItem.Use();
            }
        }

        private bool HealthOk => ObjectManager.Player.HealthPercent >= 90 || (ObjectManager.Player.HealthPercent >= 70 && !ObjectManager.Player.IsEating);

        private bool PetHealthOk => ObjectManager.Pet == null || ObjectManager.Pet.HealthPercent >= 80;

        private bool ManaOk => (ObjectManager.Player.Level < 6 && ObjectManager.Player.ManaPercent > 50) || ObjectManager.Player.ManaPercent >= 90 || (ObjectManager.Player.ManaPercent >= 55 && !ObjectManager.Player.IsDrinking);

        private bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid || u.TargetGuid == ObjectManager.Pet?.Guid);

        public void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, minRange, maxRange, condition, callback, castOnSelf);

        public void TryCastSpell(string name, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, 0, int.MaxValue, condition, callback, castOnSelf);

        private void TryCastSpellInternal(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false)
        {
            if (ObjectManager.Player.IsSpellReady(name) && condition && !ObjectManager.Player.IsStunned && ((!ObjectManager.Player.IsCasting && ObjectManager.Player.ChannelingId == 0) || ObjectManager.Player.Class == Class.Warrior))
            {
                ObjectManager.Player.CastSpell(name, castOnSelf: castOnSelf);
                callback?.Invoke();
            }
        }
    }
}
