using RaidMemberBot.AI;
using RaidMemberBot.Game;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.Tasks
{
    public class AddEquipmentTask : BotTask, IBotTask
    {
        readonly int equipmentId;
        readonly int inventorySlot;

        bool oldItemPickedUp;
        bool oldItemDeleted;
        bool newItemAdded;
        bool newItemPickedUp;
        bool newItemEquipped;
        bool newItemEquipConfirmed;
        public AddEquipmentTask(IClassContainer container, Stack<IBotTask> botTasks, int equipmentId, int inventorySlot) : base(container, botTasks, TaskType.Ordinary)
        {
            this.equipmentId = equipmentId;
            this.inventorySlot = inventorySlot;
        }

        public void Update()
        {
            if (Wait.For("EquipStagger", 500))
                return;

            if (!oldItemPickedUp)
            {
                Functions.LuaCall($"PickupInventoryItem({inventorySlot})");
                oldItemPickedUp = true;
            }
            else if (!oldItemDeleted)
            {
                Functions.LuaCall($"DeleteCursorItem()");
                oldItemDeleted = true;
                if(equipmentId == 0)
                {
                    BotTasks.Pop();
                    return;
                }
            }
            else if (!newItemAdded)
            {
                Functions.LuaCall($"SendChatMessage(\".additem {equipmentId}\")");
                newItemAdded = true;
            }
            else if (!newItemPickedUp)
            {
                WoWItem woWItem = Inventory.GetAllItems().First(x => x.ItemId == equipmentId);
                Functions.LuaCall($"PickupContainerItem({Inventory.GetBagId(woWItem.Guid)},{Inventory.GetSlotId(woWItem.Guid)})");
                newItemPickedUp = true;
            }
            else if (!newItemEquipped)
            {
                Functions.LuaCall($"EquipCursorItem({inventorySlot})");
                newItemEquipped = true;
            }
            else if (!newItemEquipConfirmed)
            {
                Functions.LuaCall($"AutoEquipCursorItem()");
                Functions.LuaCall($"StaticPopup1Button1:Click()");
                newItemEquipConfirmed = true;
            }

            if (oldItemPickedUp && oldItemDeleted && newItemAdded && newItemPickedUp && newItemEquipped && newItemEquipConfirmed)
            {
                BotTasks.Pop();
            }
        }
    }
}