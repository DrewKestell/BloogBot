using WoWSlimClient.Frames;
using WoWSlimClient.Manager;
using WoWSlimClient.Models;

namespace WoWSlimClient.Tasks.SharedStates
{
    public class QueueForBattlegroundTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit woWUnit) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private readonly WoWUnit npc = woWUnit;
        private DialogFrame dialogFrame;
        private bool hasInteracted;
        private bool hasDialogFrame;
        private bool hasSelectedGossipOption;
        private bool hasQueued;
        private readonly bool readyToPop;

        public void Update()
        {
            if (npc.Position.DistanceTo(ObjectManager.Instance.Player.Position) > 5)
            {
                ObjectManager.Instance.Player.MoveToward(npc.Position);
            }
            else if (!hasInteracted)
            {
                npc.Interact();
                hasInteracted = true;
            }
            else if (!hasDialogFrame && Wait.For("PrepDialogFrameDelay", 1000))
            {
                dialogFrame = new DialogFrame();
                hasDialogFrame = true;
            }
            else if (!hasSelectedGossipOption && Wait.For("PrepBattleMasterDialogFrameDelay", 1000))
            {
                dialogFrame.SelectFirstGossipOfType(DialogType.battlemaster);
                hasSelectedGossipOption = true;
            }
            else if (!hasQueued && ObjectManager.Instance.PartyMembers.All(x => x.Position.DistanceTo(ObjectManager.Instance.Player.Position) < 5) && Wait.For("DialogFrameDelay", 1000))
            {
                //string enabled = Functions.LuaCallWithResult("{0} = BattlefieldFrameGroupJoinButton:IsEnabled()")[0];

                //if (enabled == "1")
                //    Functions.LuaCall("BattlefieldFrameGroupJoinButton:Click()");
                //else
                //    Functions.LuaCall("BattlefieldFrameJoinButton:Click()");
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
