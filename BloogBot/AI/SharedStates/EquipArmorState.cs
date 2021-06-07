using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.AI.SharedStates
{
    public class EquipArmorState : IBotState
    {
        static readonly IDictionary<Class, ItemSubclass> desiredArmorTypes = new Dictionary<Class, ItemSubclass>
        {
            { Class.Druid, ItemSubclass.Leather },
            { Class.Hunter, ItemSubclass.Mail },
            { Class.Mage, ItemSubclass.Cloth },
            { Class.Paladin, ItemSubclass.Mail },
            { Class.Priest, ItemSubclass.Cloth },
            { Class.Rogue, ItemSubclass.Leather },
            { Class.Shaman, ItemSubclass.Leather },
            { Class.Warlock, ItemSubclass.Cloth },
            { Class.Warrior, ItemSubclass.Mail }
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

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;

        EquipSlot? emptySlot;
        WoWItem itemToEquip;

        public EquipArmorState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if (player.IsInCombat)
            {
                botStates.Pop();
                return;
            }

            if (itemToEquip == null)
            {
                foreach (var slot in slotsToCheck)
                {
                    var equippedItem = Inventory.GetEquippedItem(slot);
                    if (equippedItem == null)
                    {
                        emptySlot = slot;
                        break;
                    }
                }

                if (emptySlot != null)
                {
                    slotsToCheck.Remove(emptySlot.Value);

                    itemToEquip = Inventory.GetAllItems()
                        .FirstOrDefault(i =>
                            (i.Info.ItemSubclass == desiredArmorTypes[player.Class] || i.Info.ItemSubclass == ItemSubclass.Cloth && i.Info.EquipSlot == EquipSlot.Back) &&
                            i.Info.EquipSlot.ToString() == emptySlot.ToString() &&
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
                botStates.Pop();
                botStates.Push(container.CreateRestState(botStates, container));
                return;
            }

            if (itemToEquip != null && Wait.For("EquipItemDelay", 500))
            {
                var bagId = Inventory.GetBagId(itemToEquip.Guid);
                var slotId = Inventory.GetSlotId(itemToEquip.Guid);

                player.LuaCall($"UseContainerItem({bagId}, {slotId})");
                if ((int)itemToEquip.Quality > 1)
                    player.LuaCall("EquipPendingItem(0)");
                emptySlot = null;
                itemToEquip = null;
            }
        }
    }
}
