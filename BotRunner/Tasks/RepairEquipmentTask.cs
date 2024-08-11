using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    public class RepairEquipmentTask(IBotContext botContext, string npcName) : BotTask(botContext), IBotTask
    {
        private readonly string npcName = npcName;
        private readonly ILocalPlayer player = botContext.ObjectManager.Player;
        private State state = State.Uninitialized;
        private IWoWUnit npc;
        private readonly IDialogFrame dialogFrame;
        private readonly IMerchantFrame merchantFrame;

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
                if (merchantFrame.Ready)
                    state = State.Initialized;
                else
                {
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
            ReadyToPop
        }
    }
}
