using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace MageArcane.Tasks
{
    internal class ConjureItemsTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        private IWoWItem foodItem;
        private IWoWItem drinkItem;

        public void Update()
        {
            if (ObjectManager.Player.IsCasting)
                return;

            if (ObjectManager.Player.ManaPercent < 20)
            {
                BotTasks.Pop();
                BotTasks.Push(new RestTask(BotContext));
                return;
            }

            if ((foodItem != null || !ObjectManager.Player.IsSpellReady(ConjureFood)) && (drinkItem != null || !ObjectManager.Player.IsSpellReady(ConjureWater)))
            {
                BotTasks.Pop();

                if (ObjectManager.Player.ManaPercent <= 80)
                    BotTasks.Push(new RestTask(BotContext));

                return;
            }

            uint foodCount = foodItem == null ? 0 : ObjectManager.GetItemCount(foodItem.ItemId);
            if ((foodItem == null || foodCount <= 2) && Wait.For("ArcaneMageConjureFood", 3000))
                ObjectManager.Player.CastSpell(ConjureFood);

            uint drinkCount = drinkItem == null ? 0 : ObjectManager.GetItemCount(drinkItem.ItemId);
            if ((drinkItem == null || drinkCount <= 2) && Wait.For("ArcaneMageConjureDrink", 3000))
                ObjectManager.Player.CastSpell(ConjureWater);
        }
    }
}
