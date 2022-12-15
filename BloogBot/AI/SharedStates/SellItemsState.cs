using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Frames;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BloogBot.AI.SharedStates
{
    public class SellItemsState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly string npcName;
        readonly LocalPlayer player;
        readonly IEnumerable<WoWItem> itemsToSell;

        State state = State.Uninitialized;
        WoWUnit npc;
        DialogFrame dialogFrame;
        MerchantFrame merchantFrame;
        int itemIndex;

        public SellItemsState(Stack<IBotState> botStates, IDependencyContainer container, string npcName)
        {
            this.botStates = botStates;
            this.npcName = npcName;
            player = ObjectManager.Player;

            itemsToSell = Inventory
                .GetAllItems()
                .Where(i =>
                    (i.Info.Name != "Hearthstone") &&
                    (i.Info.Name != container.BotSettings.Food) &&
                    (i.Info.Name != container.BotSettings.Drink) &&
                    (i.Info.ItemClass != ItemClass.Quest) &&
                    (i.Info.ItemClass != ItemClass.Container) &&
                    (string.IsNullOrWhiteSpace(container.BotSettings.SellExcludedNames) || !container.BotSettings.SellExcludedNames.Split('|').Any(n => n == i.Info.Name)) &&
                    (container.BotSettings.SellUncommon ? (int)i.Quality < 3 : (int)i.Quality < 2)
                );
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
                state = State.ReadyToSell;
            }
            if (state == State.ReadyToSell)
            {
                if (Wait.For("SellItemDelay", 200))
                {
                    var itemToSell = itemsToSell.ElementAt(itemIndex);
                    merchantFrame.SellItemByGuid((uint)itemToSell.StackCount, npc.Guid, itemToSell.Guid);

                    itemIndex++;

                    if (itemIndex == itemsToSell.Count())
                    {
                        state = State.CloseMerchantFrame;
                    }
                }
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
            ReadyToPop,
            ReadyToSell
        }
    }
}
