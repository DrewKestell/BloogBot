using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;
using WoWActivityMember.Tasks;

namespace MageArcane.Tasks
{
    internal class ConjureItemsTask(Stack<IBotTask> botTasks, IClassContainer container) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private const string ConjureFood = "Conjure Food";
        private const string ConjureWater = "Conjure Water";
        private WoWItem foodItem;
        private WoWItem drinkItem;

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

        private void TryCastSpell(string name)
        {
            if (ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.IsCasting)
                Functions.LuaCall($"CastSpellByName('{name}')");
        }
    }
}
