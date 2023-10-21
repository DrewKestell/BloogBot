using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
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

            if (Container.Player.IsCasting)
                return;

            //Container.Player.Stand();

            if (Container.Player.ManaPercent < 20)
            {
                BotTasks.Pop();
                BotTasks.Push(new RestTask(container, botTasks));
                return;
            }

            if (Inventory.Instance.CountFreeSlots(false) == 0 || (foodItem != null || !Spellbook.Instance.IsSpellReady(ConjureFood)) && (drinkItem != null || !Spellbook.Instance.IsSpellReady(ConjureWater)))
            {
                BotTasks.Pop();

                if (Container.Player.ManaPercent <= 70)
                    BotTasks.Push(new RestTask(container, botTasks));

                return;
            }

            var foodCount = foodItem == null ? 0 : Inventory.Instance.GetItemCount(foodItem.Id);
            if (foodItem == null || foodCount <= 2)
                TryCastSpell(ConjureFood);

            var drinkCount = drinkItem == null ? 0 : Inventory.Instance.GetItemCount(drinkItem.Id);
            if (drinkItem == null || drinkCount <= 2)
                TryCastSpell(ConjureWater);
        }

        void TryCastSpell(string name)
        {
            if (Spellbook.Instance.IsSpellReady(name) && Container.Player.IsCasting)
                Lua.Instance.Execute($"CastSpellByName('{name}')");
        }
    }
}
