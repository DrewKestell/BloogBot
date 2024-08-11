
using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    public class AddEquipmentTask(IBotContext botContext, uint equipmentId, uint inventorySlot) : BotTask(botContext), IBotTask
    {
        private readonly uint equipmentId = equipmentId;
        private readonly uint inventorySlot = inventorySlot;
        private bool oldItemPickedUp;
        private bool oldItemDeleted;
        private bool newItemAdded;
        private bool newItemPickedUp;
        private bool newItemEquipped;
        private bool newItemEquipConfirmed;

        public void Update()
        {
            if (Wait.For("EquipStagger", 500))
                return;

            if (!oldItemPickedUp)
            {
                ObjectManager.PickupInventoryItem(inventorySlot);
                oldItemPickedUp = true;
            }
            else if (!oldItemDeleted)
            {
                ObjectManager.DeleteCursorItem();
                oldItemDeleted = true;
                if (equipmentId == 0)
                {
                    BotTasks.Pop();
                    return;
                }
            }
            else if (!newItemAdded)
            {
                ObjectManager.SendChatMessage($".additem {equipmentId}");
                newItemAdded = true;
            }
            else if (!newItemPickedUp)
            {
                IWoWItem woWItem = ObjectManager.Items.First(x => x.ItemId == equipmentId);
                newItemPickedUp = true;
            }
            else if (!newItemEquipped)
            {
                ObjectManager.EquipCursorItem();
                newItemEquipped = true;
            }
            else if (!newItemEquipConfirmed)
            {
                ObjectManager.ConfirmItemEquip();
                newItemEquipConfirmed = true;
            }

            if (oldItemPickedUp && oldItemDeleted && newItemAdded && newItemPickedUp && newItemEquipped && newItemEquipConfirmed)
            {
                BotTasks.Pop();
            }
        }
    }
}