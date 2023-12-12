using RaidMemberBot.AI;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Mem;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace AfflictionWarlockBot
{
    class RestTask : BotTask, IBotTask
    {
        const int stackCount = 5;

        const string ConsumeShadows = "Consume Shadows";
        const string HealthFunnel = "Health Funnel";
        const string LifeTap = "Life Tap";

        readonly WoWItem foodItem;
        readonly WoWItem drinkItem;
        LocalPet pet;
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest)
        {
            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            Functions.LuaCall($"SendChatMessage('.repairitems')");

            List<WoWItem> foodItems = ObjectManager.Items.Where(x => x.ItemId == 5479).ToList();
            int foodItemsCount = foodItems.Sum(x => x.StackCount);
            if (foodItemsCount < 20)
            {
                Functions.LuaCall($"SendChatMessage('.additem 5479 {20 - foodItemsCount}')");
            }
            foodItem = ObjectManager.Items.First(x => x.ItemId == 5479);
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
            pet = ObjectManager.Pet;

            if (pet != null && pet.HealthPercent < 60 && pet.CanUse(ConsumeShadows) && pet.IsCasting && pet.ChannelingId == 0)
                pet.Cast(ConsumeShadows);

            if (InCombat || (HealthOk && ManaOk))
            {
                if (ObjectManager.Player.IsCasting && ObjectManager.Player.ChannelingId == 0)
                    ObjectManager.Player.Stand();

                if (InCombat || PetHealthOk)
                {
                    pet?.FollowPlayer();
                    BotTasks.Pop();

                    BotTasks.Push(new SummonVoidwalkerTask(Container, BotTasks));
                }
                else
                {
                    if (ObjectManager.Player.ChannelingId == 0 && ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(HealthFunnel) && ObjectManager.Player.HealthPercent > 30)
                        Functions.LuaCall($"CastSpellByName('{HealthFunnel}')");
                }

                return;
            }

            if (foodItem != null && !ObjectManager.Player.IsEating && ObjectManager.Player.HealthPercent < 80 && Wait.For("EatDelay", 500, true))
                foodItem.Use();

            if (drinkItem != null && !ObjectManager.Player.IsDrinking && ObjectManager.Player.ManaPercent < 60 && Wait.For("DrinkDelay", 500, true))
                drinkItem.Use();

            TryCastSpell(LifeTap, 0, int.MaxValue, ObjectManager.Player.HealthPercent > 85 && ObjectManager.Player.ManaPercent < 80);
        }

        bool HealthOk => foodItem == null || ObjectManager.Player.HealthPercent >= 90 || (ObjectManager.Player.HealthPercent >= 70 && !ObjectManager.Player.IsEating);

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
