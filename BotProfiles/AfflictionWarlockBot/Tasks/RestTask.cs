using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
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
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest) { }

        public void Update()
        {
            pet = ObjectManager.Instance.Pet;

            if (pet != null && pet.HealthPercent < 60 && pet.CanUse(ConsumeShadows) && pet.IsCasting && pet.Channeling == 0)
                pet.Cast(ConsumeShadows);

            if (InCombat || (HealthOk && ManaOk))
            {
                if (Container.Player.IsCasting && Container.Player.Channeling == 0)
                    Container.Player.Stand();

                if (InCombat || PetHealthOk)
                {
                    pet?.FollowPlayer();
                    BotTasks.Pop();

                    var foodCount = foodItem == null ? 0 : Inventory.Instance.GetItemCount(foodItem.Id);
                    var drinkCount = drinkItem == null ? 0 : Inventory.Instance.GetItemCount(drinkItem.Id);

                    if (!InCombat && (foodCount == 0 || drinkCount == 0))
                    {
                        var foodToBuy = 12 - (foodCount / stackCount);
                        var drinkToBuy = 28 - (drinkCount / stackCount);

                        var itemsToBuy = new Dictionary<string, int>();
                        //if (foodToBuy > 0)
                        //    itemsToBuy.Add(container.BotSettings.Food, foodToBuy);
                        //if (drinkToBuy > 0)
                        //    itemsToBuy.Add(container.BotSettings.Drink, drinkToBuy);

                        //var currentHotspot = container.GetCurrentHotspot();
                        //if (currentHotspot.TravelPath != null)
                        //{
                        //    BotTasks.Push(new TravelState(botTasks, container, currentHotspot.TravelPath.Waypoints, 0));
                        //    BotTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.TravelPath.Waypoints[0]));
                        //}

                        //BotTasks.Push(new BuyItemsState(botTasks, currentHotspot.Innkeeper.Name, itemsToBuy));
                        //BotTasks.Push(new SellItemsState(botTasks, container, currentHotspot.Innkeeper.Name));
                        //BotTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.Innkeeper.Location));
                        //container.CheckForTravelPath(botTasks, true, false);
                    }
                    else
                        BotTasks.Push(new SummonVoidwalkerTask(Container, BotTasks));
                }
                else
                {
                    if (Container.Player.Channeling == 0 && Container.Player.IsCasting && Spellbook.Instance.IsSpellReady(HealthFunnel) && Container.Player.HealthPercent > 30)
                        Lua.Instance.Execute($"CastSpellByName('{HealthFunnel}')");
                }

                return;
            }

            if (foodItem != null && !Container.Player.IsEating && Container.Player.HealthPercent < 80 && Wait.For("EatDelay", 500, true))
                foodItem.Use();

            if (drinkItem != null && !Container.Player.IsDrinking && Container.Player.ManaPercent < 60 && Wait.For("DrinkDelay", 500, true))
                drinkItem.Use();

            TryCastSpell(LifeTap, 0, int.MaxValue, ObjectManager.Instance.Player.HealthPercent > 85 && ObjectManager.Instance.Player.ManaPercent < 80);
        }

        bool HealthOk => foodItem == null || Container.Player.HealthPercent >= 90 || (Container.Player.HealthPercent >= 70 && !Container.Player.IsEating);

        bool PetHealthOk => ObjectManager.Instance.Pet == null || ObjectManager.Instance.Pet.HealthPercent >= 80;

        bool ManaOk => (Container.Player.Level < 6 && Container.Player.ManaPercent > 50) || Container.Player.ManaPercent >= 90 || (Container.Player.ManaPercent >= 55 && !Container.Player.IsDrinking);

        bool InCombat => Container.Player.IsInCombat || ObjectManager.Instance.Units.Any(u => u.TargetGuid == Container.Player.Guid || u.TargetGuid == ObjectManager.Instance.Pet?.Guid);
        
        public void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, minRange, maxRange, condition, callback, castOnSelf);

        public void TryCastSpell(string name, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, 0, int.MaxValue, condition, callback, castOnSelf);

        void TryCastSpellInternal(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false)
        {
            if (Spellbook.Instance.IsSpellReady(name) && condition && !Container.Player.IsStunned && ((!Container.Player.IsCasting && !Container.Player.IsChanneling) || Container.Player.Class == ClassId.Warrior))
            {
                Spellbook.Instance.Cast(name);
                callback?.Invoke();
            }
        }
    }
}
