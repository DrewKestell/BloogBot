using RaidMemberBot.Game.Frames;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Models;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.AI.SharedStates
{
    public class SellItemsTask : BotTask, IBotTask
    {
        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly string npcName;
        readonly LocalPlayer player;
        readonly IEnumerable<WoWItem> itemsToSell;

        State state = State.Uninitialized;
        Location npcLocation;
        WoWUnit npc;
        GossipFrame dialogFrame;
        MerchantFrame merchantFrame;
        int itemIndex;

        public SellItemsTask(IClassContainer container, Stack<IBotTask> botTasks, Creature vendorNpc)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.npcLocation = new Location(vendorNpc.LocationX, vendorNpc.LocationY, vendorNpc.LocationZ);
            player = ObjectManager.Instance.Player;

            itemsToSell = Inventory
                .Instance
                .GetAllItems()
                .Where(i =>
                    (i.Info.Name != "Hearthstone") 
                    //&&
                    //(i.Info.ItemClass != ItemClass.Quest) &&
                    //(i.Info.ItemClass != ItemClass.Bag)
                );
        }

        public void Update()
        {
            if (npcLocation.GetDistanceTo(player.Location) > 5)
            {
                botTasks.Push(new MoveToLocationTask(container, botTasks, npcLocation));
                return;
            }
            if (state == State.Uninitialized)
            {
                npc = ObjectManager
                    .Instance
                    .Units
                    .Single(u => u.Name == npcName);
                state = State.Interacting;
            }
            if (state == State.Interacting)
            {
                npc.Interact(false);
                state = State.PrepMerchantFrame;
            }
            if (state == State.PrepMerchantFrame && Wait.For("PrepMerchantFrameDelay", 500))
            {
                if (MerchantFrame.IsOpen)
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
                    var itemToSell = itemsToSell.ElementAt(itemIndex);
                    MerchantFrame.Instance.VendorByGuid(itemToSell.Guid);

                    itemIndex++;

                    if (itemIndex == itemsToSell.Count())
                    {
                        state = State.CloseMerchantFrame;
                    }
                }
            }
            if (state == State.Dialog && Wait.For("DialogFrameDelay", 500))
            {
                GossipFrame.Instance.SelectFirstGossipOfType(Constants.Enums.GossipTypes.Vendor);
                state = State.PrepMerchantFrame;
            }
            if (state == State.CloseMerchantFrame && Wait.For("BuyItemsCloseMerchantFrameStateDelay", 2000))
            {
                state = State.ReadyToPop;
            }
            if (state == State.ReadyToPop && Wait.For("BuyItemsPopBuyItemsStateDelay", 5000))
            {
                Wait.RemoveAll();
                botTasks.Pop();
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
