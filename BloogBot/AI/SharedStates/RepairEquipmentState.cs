using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Frames;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.AI.SharedStates
{
    public class RepairEquipmentState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly string npcName;
        readonly LocalPlayer player;

        State state = State.Uninitialized;
        WoWUnit npc;
        DialogFrame dialogFrame;
        MerchantFrame merchantFrame;

        public RepairEquipmentState(Stack<IBotState> botStates, IDependencyContainer container, string npcName)
        {
            this.botStates = botStates;
            this.container = container;
            this.npcName = npcName;
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
                merchantFrame.RepairAll();
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

        void WowEventHandler_OnDialogOpened(object sender, OnDialogFrameOpenArgs e) =>
            dialogFrame = e.DialogFrame;

        void WowEventHandler_OnMerchantFrameOpened(object sender, OnMerchantFrameOpenArgs e) =>
            merchantFrame = e.MerchantFrame;

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
}
