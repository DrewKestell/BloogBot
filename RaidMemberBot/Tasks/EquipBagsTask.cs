using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
{
    public class EquipBagsTask : BotTask, IBotTask
    {
        WoWItem newBag;

        public EquipBagsTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary) { }

        public void Update()
        {
            if (ObjectManager.Player.IsInCombat)
            {
                BotTasks.Pop();
                return;
            }

            //if (newBag == null)
            //    newBag = Inventory
            //        .Instance
            //        .GetAllItems()
            //        .FirstOrDefault(i => i.Info.ItemClass == ItemClass.Bag);

            //if (newBag == null || Inventory.EmptyBagSlots == 0)
            //{
            //    BotTasks.Pop();
            //    BotTasks.Push(new EquipArmorTask(Container, BotTasks));
            //    return;
            //}

            //var bagId = Inventory.GetBagId(newBag.Guid);
            //var slotId = Inventory.GetSlotId(newBag.Guid);

            //if (slotId == 0)
            //{
            //    newBag = null;
            //    return;
            //}

            //Functions.LuaCall($"UseContainerItem({bagId}, {slotId})");
            newBag = null;
        }
    }
}
