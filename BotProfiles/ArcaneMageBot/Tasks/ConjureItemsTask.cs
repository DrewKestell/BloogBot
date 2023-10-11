using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace ArcaneMageBot
{
    class ConjureItemsTask : IBotTask
    {
        const string ConjureFood = "Conjure Food";
        const string ConjureWater = "Conjure Water";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;

        WoWItem foodItem;
        WoWItem drinkItem;

        public ConjureItemsTask(Stack<IBotTask> botTasks, IClassContainer container)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            //foodItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Food);

            //drinkItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Drink);

            if (player.IsCasting)
                return;

            if (player.ManaPercent < 20)
            {
                botTasks.Pop();
                botTasks.Push(new RestTask(container, botTasks));
                return;
            }

            if ((foodItem != null || !Spellbook.Instance.IsSpellReady(ConjureFood)) && (drinkItem != null || !Spellbook.Instance.IsSpellReady(ConjureWater)))
            {
                botTasks.Pop();

                if (player.ManaPercent <= 80)
                    botTasks.Push(new RestTask(container, botTasks));

                return;
            }

            var foodCount = foodItem == null ? 0 : Inventory.Instance.GetItemCount(foodItem.Id);
            if ((foodItem == null || foodCount <= 2) && Wait.For("ArcaneMageConjureFood", 3000))
                TryCastSpell(ConjureFood);

            var drinkCount = drinkItem == null ? 0 : Inventory.Instance.GetItemCount(drinkItem.Id);
            if ((drinkItem == null || drinkCount <= 2) && Wait.For("ArcaneMageConjureDrink", 3000))
                TryCastSpell(ConjureWater);
        }

        void TryCastSpell(string name)
        {
            if (Spellbook.Instance.IsSpellReady(name) && player.IsCasting)
                Lua.Instance.Execute($"CastSpellByName('{name}')");
        }
    }
}
