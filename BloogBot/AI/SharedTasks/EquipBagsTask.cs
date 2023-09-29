using BloogBot.Game;
using BloogBot.Game.Objects;
using BloogBot.Models;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.AI.SharedStates
{
    public class EquipBagsTask : BotTask, IBotTask
    {
        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;

        WoWItem newBag;

        public EquipBagsTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.container = container;
            this.botTasks = botTasks;
        }

        public void Update()
        {
            if (ObjectManager.Player.IsInCombat)
            {
                botTasks.Pop();
                return;
            }

            if (newBag == null)
                newBag = Inventory
                    .GetAllItems()
                    .FirstOrDefault(i => i.Info.ItemClass == ItemClass.Bag);

            if (newBag == null || Inventory.EmptyBagSlots == 0)
            {
                botTasks.Pop();
                botTasks.Push(new EquipArmorTask(container, botTasks));
                return;
            }

            var bagId = Inventory.GetBagId(newBag.Guid);
            var slotId = Inventory.GetSlotId(newBag.Guid);

            if (slotId == 0)
            {
                newBag = null;
                return;
            }

            ObjectManager.Player.LuaCall($"UseContainerItem({bagId}, {slotId})");
            newBag = null;
        }
    }
}
