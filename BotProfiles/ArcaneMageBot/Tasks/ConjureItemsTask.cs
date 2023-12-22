using RaidMemberBot.AI;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace ArcaneMageBot
{
    class ConjureItemsTask : BotTask, IBotTask
    {
        const string ConjureFood = "Conjure Food";
        const string ConjureWater = "Conjure Water";

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

            if (ObjectManager.Player.ManaPercent < 20)
            {
                BotTasks.Pop();
                BotTasks.Push(new RestTask(Container, BotTasks));
                return;
            }

            if ((foodItem != null || !ObjectManager.Player.IsSpellReady(ConjureFood)) && (drinkItem != null || !ObjectManager.Player.IsSpellReady(ConjureWater)))
            {
                BotTasks.Pop();

                if (ObjectManager.Player.ManaPercent <= 80)
                    BotTasks.Push(new RestTask(Container, BotTasks));

                return;
            }

            int foodCount = foodItem == null ? 0 : Inventory.GetItemCount(foodItem.ItemId);
            if ((foodItem == null || foodCount <= 2) && Wait.For("ArcaneMageConjureFood", 3000))
                TryCastSpell(ConjureFood);

            int drinkCount = drinkItem == null ? 0 : Inventory.GetItemCount(drinkItem.ItemId);
            if ((drinkItem == null || drinkCount <= 2) && Wait.For("ArcaneMageConjureDrink", 3000))
                TryCastSpell(ConjureWater);
        }

        void TryCastSpell(string name)
        {
            if (ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.IsCasting)
                Functions.LuaCall($"CastSpellByName('{name}')");
        }
    }
}
