using WoWSlimClient.Manager;
using WoWSlimClient.Models;

namespace WoWSlimClient.Tasks.SharedStates
{
    public class EquipBagsTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private WoWItem newBag;

        public void Update()
        {
            if (ObjectManager.Instance.Player.IsInCombat)
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
