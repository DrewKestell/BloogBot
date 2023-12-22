using RaidMemberBot.Game;
using RaidMemberBot.Game.Frames;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Models;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.AI.SharedStates
{
    public class SellItemsTask : BotTask, IBotTask
    {
        readonly string npcName;
        readonly LocalPlayer player;
        readonly IEnumerable<WoWItem> itemsToSell;

        State state = State.Uninitialized;
        Position npcPosition;
        WoWUnit npc;
        DialogFrame dialogFrame;
        MerchantFrame merchantFrame;
        int itemIndex;

        public SellItemsTask(IClassContainer container, Stack<IBotTask> botTasks, Creature vendorNpc) : base(container, botTasks, TaskType.Ordinary)
        {
            npcPosition = new Position(vendorNpc.PositionX, vendorNpc.PositionY, vendorNpc.PositionZ);
            player = ObjectManager.Player;

            ObjectManager.Items
                .Where(i =>
                    (i.Info.Name != "Hearthstone")
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
                dialogFrame.SelectFirstGossipOfType(Constants.Enums.DialogType.vendor);
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

        enum State
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
