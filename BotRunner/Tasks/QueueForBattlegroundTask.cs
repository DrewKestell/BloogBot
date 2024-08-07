using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    public class QueueForBattlegroundTask(IBotContext botContext, IWoWUnit woWUnit) : BotTask(botContext), IBotTask
    {
        private readonly IWoWUnit npc = woWUnit;
        private readonly IDialogFrame dialogFrame;
        private bool hasInteracted;
        private bool hasDialogFrame;
        private bool hasSelectedGossipOption;
        private bool hasQueued;
        private readonly bool readyToPop;

        public void Update()
        {
            if (npc.Position.DistanceTo(ObjectManager.Player.Position) > 5)
            {
                ObjectManager.Player.MoveToward(npc.Position);
            }
            else if (!hasInteracted)
            {
                npc.Interact();
                hasInteracted = true;
            }
            else if (!hasDialogFrame && Wait.For("PrepDialogFrameDelay", 1000))
            {
                //dialogFrame = new DialogFrame();
                hasDialogFrame = true;
            }
            else if (!hasSelectedGossipOption && Wait.For("PrepBattleMasterDialogFrameDelay", 1000))
            {
                dialogFrame.SelectFirstGossipOfType(DialogType.battlemaster);
                hasSelectedGossipOption = true;
            }
            else if (!hasQueued && ObjectManager.PartyMembers.All(x => x.Position.DistanceTo(ObjectManager.Player.Position) < 5) && Wait.For("DialogFrameDelay", 1000))
            {
                ObjectManager.JoinBattleGroundQueue();
                hasQueued = true;
            }
            else if (hasQueued)
            {
                Wait.RemoveAll();
                BotTasks.Pop();
            }
        }
    }
}
