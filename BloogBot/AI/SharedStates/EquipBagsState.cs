using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.AI.SharedStates
{
    public class EquipBagsState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;

        WoWItem newBag;

        public EquipBagsState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
        }

        public void Update()
        {
            if (ObjectManager.Player.IsInCombat)
            {
                botStates.Pop();
                return;
            }

            if (newBag == null)
                newBag = Inventory
                    .GetAllItems()
                    .FirstOrDefault(i => i.Info.ItemClass == ItemClass.Container);

            if (newBag == null || Inventory.EmptyBagSlots == 0)
            {
                botStates.Pop();
                botStates.Push(new EquipArmorState(botStates, container));
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
