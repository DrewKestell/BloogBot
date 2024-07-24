using MaNGOSDBDomain.Models;
using WoWSlimClient.Frames;
using WoWSlimClient.Manager;
using WoWSlimClient.Models;

namespace WoWSlimClient.Tasks.SharedStates
{
    public class SellItemsTask : BotTask, IBotTask
    {
        private readonly string npcName;
        private readonly WoWLocalPlayer player;
        private readonly IEnumerable<WoWItem> itemsToSell;
        private State state = State.Uninitialized;
        private readonly Position npcPosition;
        private WoWUnit npc;
        private readonly DialogFrame dialogFrame;
        private readonly MerchantFrame merchantFrame;
        private int itemIndex;

        public SellItemsTask(IClassContainer container, Stack<IBotTask> botTasks, Creature vendorNpc) : base(container, botTasks, TaskType.Ordinary)
        {
            npcPosition = new Position(vendorNpc.PositionX, vendorNpc.PositionY, vendorNpc.PositionZ);
            player = ObjectManager.Instance.Player;

            ObjectManager.Instance.Items
                .Where(i =>
                    (i.Name != "Hearthstone")
                //&&
                //(i.Info.ItemClass != ItemClass.Quest) &&
                //(i.Info.ItemClass != ItemClass.Bag)
                );
        }

        public void Update()
        {
            if (npcPosition.DistanceTo(ObjectManager.Instance.Player.Position) > 5)
            {
                ObjectManager.Instance.Player.MoveToward(npcPosition);
                return;
            }
            if (state == State.Uninitialized)
            {
                npc = ObjectManager.Instance
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
                    WoWItem itemToSell = itemsToSell.ElementAt(itemIndex);
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
