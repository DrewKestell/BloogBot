using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    public class EquipBagsTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        private IWoWItem newBag;

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
