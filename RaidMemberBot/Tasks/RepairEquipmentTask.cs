using RaidMemberBot.Constants;
using RaidMemberBot.Game.Frames;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.AI.SharedStates
{
    public class RepairEquipmentTask : BotTask, IBotTask
    {
        readonly string npcName;
        readonly LocalPlayer player;

        State state = State.Uninitialized;
        WoWUnit npc;

        public RepairEquipmentTask(IClassContainer container, Stack<IBotTask> botTasks, string npcName) : base(container, botTasks, TaskType.Ordinary)
        {
            this.npcName = npcName;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
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
                MerchantFrame.Instance.RepairAll();
                state = State.CloseMerchantFrame;
            }
            if (state == State.Dialog && Wait.For("DialogFrameDelay", 500))
            {
                GossipFrame.Instance.SelectFirstGossipOfType(Enums.GossipTypes.Vendor);
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
            ReadyToPop
        }
    }
}
