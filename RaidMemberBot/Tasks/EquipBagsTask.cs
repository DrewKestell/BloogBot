using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
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
            if (ObjectManager.Instance.Player.IsInCombat)
            {
                botTasks.Pop();
                return;
            }

            //if (newBag == null)
            //    newBag = Inventory
            //        .Instance
            //        .GetAllItems()
            //        .FirstOrDefault(i => i.Info.ItemClass == ItemClass.Bag);

            //if (newBag == null || Inventory.Instance.EmptyBagSlots == 0)
            //{
            //    botTasks.Pop();
            //    botTasks.Push(new EquipArmorTask(container, botTasks));
            //    return;
            //}

            //var bagId = Inventory.GetBagId(newBag.Guid);
            //var slotId = Inventory.GetSlotId(newBag.Guid);

            //if (slotId == 0)
            //{
            //    newBag = null;
            //    return;
            //}

            //Lua.Instance.Execute($"UseContainerItem({bagId}, {slotId})");
            newBag = null;
        }
    }
}
