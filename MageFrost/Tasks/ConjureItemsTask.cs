using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;
using WoWActivityMember.Tasks;

namespace MageFrost.Tasks
{
    internal class ConjureItemsTask : BotTask, IBotTask
    {
        private const string ConjureFood = "Conjure Food";
        private const string ConjureWater = "Conjure Water";
        private readonly Stack<IBotTask> botTasks;
        private readonly IClassContainer container;
        private readonly LocalPlayer player;
        private WoWItem foodItem;
        private WoWItem drinkItem;

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

        private void TryCastSpell(string name)
        {
            if (ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.IsCasting)
                Functions.LuaCall($"CastSpellByName('{name}')");
        }
    }
}
