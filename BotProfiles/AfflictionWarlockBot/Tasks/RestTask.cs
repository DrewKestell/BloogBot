using RaidMemberBot.AI;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace AfflictionWarlockBot
{
    class RestTask : BotTask, IBotTask
    {
        const string ConsumeShadows = "Consume Shadows";
        const string HealthFunnel = "Health Funnel";
        const string LifeTap = "Life Tap";

        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest) { }

        public void Update()
        {
            if (ObjectManager.Pet != null && ObjectManager.Pet.HealthPercent < 60 && ObjectManager.Pet.CanUse(ConsumeShadows) && ObjectManager.Pet.IsCasting && ObjectManager.Pet.ChannelingId == 0)
                ObjectManager.Pet.Cast(ConsumeShadows);

            if (InCombat || (HealthOk && ManaOk))
            {
                if (ObjectManager.Player.IsCasting && ObjectManager.Player.ChannelingId == 0)
                    ObjectManager.Player.Stand();

                if (InCombat || PetHealthOk)
                {
                    ObjectManager.Pet?.FollowPlayer();
                    BotTasks.Pop();

                    BotTasks.Push(new SummonPetTask(Container, BotTasks));
                }
                else
                {
                    if (ObjectManager.Player.ChannelingId == 0 && ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(HealthFunnel) && ObjectManager.Player.HealthPercent > 30)
                        Functions.LuaCall($"CastSpellByName('{HealthFunnel}')");
                }

                return;
            }

            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            TryCastSpell(LifeTap, 0, int.MaxValue, ObjectManager.Player.HealthPercent > 85 && ObjectManager.Player.ManaPercent < 80);

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

                WoWItem foodItem = ObjectManager.Items.First(x => x.ItemId == 5479);

                List<WoWItem> drinkItems = ObjectManager.Items.Where(x => x.ItemId == 1179).ToList();
                int drinkItemsCount = drinkItems.Sum(x => x.StackCount);

                if (drinkItemsCount < 20)
                {
                    Functions.LuaCall($"SendChatMessage('.additem 1179 {20 - drinkItemsCount}')");
                }

                WoWItem drinkItem = ObjectManager.Items.First(x => x.ItemId == 1179);

                if (foodItem != null && !ObjectManager.Player.IsEating && ObjectManager.Player.HealthPercent < 80 && Wait.For("EatDelay", 500, true))
                    foodItem.Use();

                if (drinkItem != null && !ObjectManager.Player.IsDrinking && ObjectManager.Player.ManaPercent < 60 && Wait.For("DrinkDelay", 500, true))
                    drinkItem.Use();
            }
        }

        bool HealthOk => ObjectManager.Player.HealthPercent >= 90 || (ObjectManager.Player.HealthPercent >= 70 && !ObjectManager.Player.IsEating);

        bool PetHealthOk => ObjectManager.Pet == null || ObjectManager.Pet.HealthPercent >= 80;

        bool ManaOk => (ObjectManager.Player.Level < 6 && ObjectManager.Player.ManaPercent > 50) || ObjectManager.Player.ManaPercent >= 90 || (ObjectManager.Player.ManaPercent >= 55 && !ObjectManager.Player.IsDrinking);

        bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid || u.TargetGuid == ObjectManager.Pet?.Guid);

        public void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, minRange, maxRange, condition, callback, castOnSelf);

        public void TryCastSpell(string name, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, 0, int.MaxValue, condition, callback, castOnSelf);

        void TryCastSpellInternal(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false)
        {
            if (ObjectManager.Player.IsSpellReady(name) && condition && !ObjectManager.Player.IsStunned && ((!ObjectManager.Player.IsCasting && ObjectManager.Player.ChannelingId == 0) || ObjectManager.Player.Class == Class.Warrior))
            {
                Functions.LuaCall($"CastSpellByName('{name}')");
                callback?.Invoke();
            }
        }
    }
}
