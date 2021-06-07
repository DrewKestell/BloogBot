using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Frames;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BloogBot.AI.SharedStates
{
    public class BuyItemsState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly string npcName;
        readonly LocalPlayer player;
        readonly IDictionary<string, int> itemsToBuy;

        State state = State.Uninitialized;
        WoWUnit npc;
        DialogFrame dialogFrame;
        MerchantFrame merchantFrame;
        
        public BuyItemsState(Stack<IBotState> botStates, string npcName, IDictionary<string, int> itemsToBuy)
        {
            this.botStates = botStates;
            this.npcName = npcName;
            this.itemsToBuy = itemsToBuy;
            player = ObjectManager.Player;
        }

        public void Update()
        {
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
                merchantFrame = new MerchantFrame();

                if (merchantFrame.Ready)
                    state = State.Initialized;
                else
                {
                    dialogFrame = new DialogFrame();
                    state = State.Dialog;
                }
            }
            if (state == State.Initialized && Wait.For("InitializeDelay", 500))
            {
                foreach (var item in itemsToBuy)
                {
                    merchantFrame.BuyItemByName(npc.Guid, item.Key, item.Value);
                    Thread.Sleep(200);
                }

                state = State.CloseMerchantFrame;
            }
            if (state == State.Dialog && Wait.For("DialogFrameDelay", 500))
            {
                dialogFrame.SelectFirstGossipOfType(player, DialogType.vendor);
                state = State.PrepMerchantFrame;
            }
            if (state == State.CloseMerchantFrame && Wait.For("BuyItemsCloseMerchantFrameStateDelay", 2000))
            {
                merchantFrame.CloseMerchantFrame();
                state = State.ReadyToPop;
            }
            if (state == State.ReadyToPop && Wait.For("BuyItemsPopBuyItemsStateDelay", 5000))
            {
                Wait.RemoveAll();
                botStates.Pop();
            }
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
        ReadyToPop
    }
}