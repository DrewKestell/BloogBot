using RaidMemberBot.AI;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace FrostMageBot
{
    class ConjureItemsTask : BotTask, IBotTask
    {
        const string ConjureFood = "Conjure Food";
        const string ConjureWater = "Conjure Water";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;

        WoWItem foodItem;
        WoWItem drinkItem;

        public ConjureItemsTask(Stack<IBotTask> botTasks, IClassContainer container) : base(container, botTasks, TaskType.Ordinary) { }

        public void Update()
        {
            //foodItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Food);

            //drinkItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Drink);

            if (ObjectManager.Player.IsCasting)
                return;

            //ObjectManager.Player.Stand();

            if (ObjectManager.Player.ManaPercent < 20)
            {
                BotTasks.Pop();
                BotTasks.Push(new RestTask(container, botTasks));
                return;
            }

            if (Inventory.CountFreeSlots(false) == 0 || (foodItem != null || !ObjectManager.Player.IsSpellReady(ConjureFood)) && (drinkItem != null || !ObjectManager.Player.IsSpellReady(ConjureWater)))
            {
                BotTasks.Pop();

                if (ObjectManager.Player.ManaPercent <= 70)
                    BotTasks.Push(new RestTask(container, botTasks));

                return;
            }

            int foodCount = foodItem == null ? 0 : Inventory.GetItemCount(foodItem.ItemId);
            if (foodItem == null || foodCount <= 2)
                TryCastSpell(ConjureFood);

            int drinkCount = drinkItem == null ? 0 : Inventory.GetItemCount(drinkItem.ItemId);
            if (drinkItem == null || drinkCount <= 2)
                TryCastSpell(ConjureWater);
        }

        void TryCastSpell(string name)
        {
            if (ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.IsCasting)
                Functions.LuaCall($"CastSpellByName('{name}')");
        }
    }
}
