using BotRunner.Interfaces;
using Database;
using PathfindingService.Models;

namespace BotRunner.Tasks
{
    public class SellItemsTask : BotTask, IBotTask
    {
        private readonly string npcName;
        private readonly ILocalPlayer player;
        private readonly IEnumerable<IWoWItem> itemsToSell;
        private State state = State.Uninitialized;
        private readonly Position npcPosition;
        private IWoWUnit npc;
        private readonly IDialogFrame dialogFrame;
        private readonly IMerchantFrame merchantFrame;
        private int itemIndex;

        public SellItemsTask(IBotContext botContext, Creature vendorNpc) : base(botContext)
        {
            npcPosition = new Position(vendorNpc.PositionX, vendorNpc.PositionY, vendorNpc.PositionZ);
            player = ObjectManager.Player;

            ObjectManager.Items
                .Where(i =>
                    i.Info.Name != "Hearthstone"
                //&&
                //(i.Info.ItemClass != ItemClass.Quest) &&
                //(i.Info.ItemClass != ItemClass.Bag)
                );
        }

        public void Update()
        {
            if (npcPosition.DistanceTo(ObjectManager.Player.Position) > 5)
            {
                ObjectManager.Player.MoveToward(npcPosition);
                return;
            }
            if (state == State.Uninitialized)
            {
                npc = ObjectManager
                    .Units
                    .Single(u => u.Name == npcName);
                state = State.Interacting;
            }
            if (state == State.Interacting)
            {
                npc.Interact();
                state = State.PrepMerchantFrame;
            }
            if (state == State.PrepMerchantFrame && Wait.For("PrepMerchantFrameDelay", 500))
            {
                if (merchantFrame.Ready)
                    state = State.Initialized;
                else
                {
                    state = State.Dialog;
                }
            }
            if (state == State.Initialized && Wait.For("InitializeDelay", 500))
            {
                state = State.ReadyToSell;
            }
            if (state == State.ReadyToSell)
            {
                if (Wait.For("SellItemDelay", 200))
                {
                    IWoWItem itemToSell = itemsToSell.ElementAt(itemIndex);
                    merchantFrame.SellItemByGuid(1, npc.Guid, itemToSell.Guid);

                    itemIndex++;

                    if (itemIndex == itemsToSell.Count())
                    {
                        state = State.CloseMerchantFrame;
                    }
                }
            }
            if (state == State.Dialog && Wait.For("DialogFrameDelay", 500))
            {
                dialogFrame.SelectFirstGossipOfType(DialogType.vendor);
                state = State.PrepMerchantFrame;
            }
            if (state == State.CloseMerchantFrame && Wait.For("BuyItemsCloseMerchantFrameStateDelay", 2000))
            {
                state = State.ReadyToPop;
            }
            if (state == State.ReadyToPop && Wait.For("BuyItemsPopBuyItemsStateDelay", 5000))
            {
                Wait.RemoveAll();
                BotTasks.Pop();
            }
        }

        private enum State
        {
            Uninitialized,
            Interacting,
            PrepMerchantFrame,
            Initialized,
            Dialog,
            CloseMerchantFrame,
            ReadyToPop,
            ReadyToSell
        }
    }
}
