using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AfflictionWarlockBot
{
    class RestTask : IBotTask
    {
        const int stackCount = 5;

        const string ConsumeShadows = "Consume Shadows";
        const string HealthFunnel = "Health Funnel";

        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;
        
        readonly WoWItem foodItem;
        readonly WoWItem drinkItem;
        LocalPet pet;

        public RestTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.container = container;
            this.botTasks = botTasks;
            player = ObjectManager.Instance.Player;
            player.SetTarget(player.Guid);

            //foodItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Food);

            //drinkItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Drink);
        }

        public void Update()
        {
            pet = ObjectManager.Instance.Pet;

            if (pet != null && pet.HealthPercent < 60 && pet.CanUse(ConsumeShadows) && pet.Casting == 0 && pet.Channeling == 0)
                pet.Cast(ConsumeShadows);

            if (InCombat || (HealthOk && ManaOk))
            {
                if (player.Casting == 0 && player.Channeling == 0)
                    player.Stand();

                if (InCombat || PetHealthOk)
                {
                    pet?.FollowPlayer();
                    botTasks.Pop();

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
                        //    botTasks.Push(new TravelState(botTasks, container, currentHotspot.TravelPath.Waypoints, 0));
                        //    botTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.TravelPath.Waypoints[0]));
                        //}

                        //botTasks.Push(new BuyItemsState(botTasks, currentHotspot.Innkeeper.Name, itemsToBuy));
                        //botTasks.Push(new SellItemsState(botTasks, container, currentHotspot.Innkeeper.Name));
                        //botTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.Innkeeper.Location));
                        //container.CheckForTravelPath(botTasks, true, false);
                    }
                    else
                        botTasks.Push(new SummonVoidwalkerTask(container, botTasks));
                }
                else
                {
                    if (player.Channeling == 0 && player.Casting == 0 && Spellbook.Instance.IsSpellReady(HealthFunnel) && player.HealthPercent > 30)
                        Lua.Instance.Execute($"CastSpellByName('{HealthFunnel}')");
                }

                return;
            }

            if (foodItem != null && !player.IsEating && player.HealthPercent < 80 && Wait.For("EatDelay", 500, true))
                foodItem.Use();

            if (drinkItem != null && !player.IsDrinking && player.ManaPercent < 60 && Wait.For("DrinkDelay", 500, true))
                drinkItem.Use();
        }

        bool HealthOk => foodItem == null || player.HealthPercent >= 90 || (player.HealthPercent >= 70 && !player.IsEating);

        bool PetHealthOk => ObjectManager.Instance.Pet == null || ObjectManager.Instance.Pet.HealthPercent >= 80;

        bool ManaOk => (player.Level < 6 && player.ManaPercent > 50) || player.ManaPercent >= 90 || (player.ManaPercent >= 55 && !player.IsDrinking);

        bool InCombat => ObjectManager.Instance.Player.IsInCombat || ObjectManager.Instance.Units.Any(u => u.TargetGuid == ObjectManager.Instance.Player.Guid || u.TargetGuid == ObjectManager.Instance.Pet?.Guid);
    }
}
