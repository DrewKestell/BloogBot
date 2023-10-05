using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.AI.SharedStates
{
    public class EquipArmorTask : BotTask, IBotTask
    {
        static readonly IDictionary<ClassId, ItemClass> desiredArmorTypes = new Dictionary<ClassId, ItemClass>
        {
            { ClassId.Druid, ItemClass.Leather },
            { ClassId.Hunter, ItemClass.Mail },
            { ClassId.Mage, ItemClass.Cloth },
            { ClassId.Paladin, ItemClass.Mail },
            { ClassId.Priest, ItemClass.Cloth },
            { ClassId.Rogue, ItemClass.Leather },
            { ClassId.Shaman, ItemClass.Leather },
            { ClassId.Warlock, ItemClass.Cloth },
            { ClassId.Warrior, ItemClass.Mail }
        };

        readonly IList<EquipSlot> slotsToCheck = new List<EquipSlot>
        {
            EquipSlot.Back,
            EquipSlot.Chest,
            EquipSlot.Feet,
            EquipSlot.Hands,
            EquipSlot.Head,
            EquipSlot.Legs,
            EquipSlot.Shoulders,
            EquipSlot.Waist,
            EquipSlot.Wrist
        };

        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;

        EquipSlot? emptySlot;
        WoWItem itemToEquip;

        public EquipArmorTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.container = container;
            this.botTasks = botTasks;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if (player.IsInCombat)
            {
                botTasks.Pop();
                return;
            }

            if (itemToEquip == null)
            {
                foreach (var slot in slotsToCheck)
                {
                    var equippedItem = Inventory.Instance.GetEquippedItem(slot);
                    if (equippedItem == null)
                    {
                        emptySlot = slot;
                        break;
                    }
                }

                if (emptySlot != null)
                {
                    slotsToCheck.Remove(emptySlot.Value);

                    itemToEquip = Inventory.Instance.GetAllItems()
                        .FirstOrDefault(i =>
                            //(i.Info.ItemSubclass == desiredArmorTypes[player.Class] || i.Info.ItemClass == ItemClass.Cloth && i.Info.EquipSlot == EquipSlot.Back) &&
                            //i.Info.EquipSlot.ToString() == emptySlot.ToString() &&
                            i.Info.RequiredLevel <= player.Level
                        );

                    if (itemToEquip == null)
                        emptySlot = null;
                }
                else
                    slotsToCheck.Clear();
            }

            if (itemToEquip == null && slotsToCheck.Count == 0)
            {
                botTasks.Pop();
                return;
            }

            if (itemToEquip != null && Wait.For("EquipItemDelay", 500))
            {
                //var bagId = Inventory.Instance.GetBagId(itemToEquip.Guid);
                //var slotId = Inventory.GetSlotId(itemToEquip.Guid);

                //Lua.Instance.Execute($"UseContainerItem({bagId}, {slotId})");
                //if ((int)itemToEquip.Quality > 1)
                //    Lua.Instance.Execute("EquipPendingItem(0)");
                //emptySlot = null;
                //itemToEquip = null;
            }
        }
    }
}
