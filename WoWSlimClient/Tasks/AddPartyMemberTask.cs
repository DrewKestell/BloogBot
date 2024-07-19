using WoWSlimClient.Models;

namespace WoWSlimClient.Tasks.SharedStates
{
    public class AddPartyMemberTask(IClassContainer container, Stack<IBotTask> botTasks, string characterName) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private readonly string characterName = characterName;
        private bool hasInvited;

        public void Update()
        {
            if (hasInvited && Wait.For("GroupInviteDelay", 500))
            {
                BotTasks.Pop();
                return;
            }
            else if(!hasInvited)
            {
                //if (ObjectManager.PartyMembers.Count() > 4 && !Container.State.InRaid)
                //{
                //    Functions.LuaCall("ConvertToRaid()");
                //}
            } 
        }
    }
}
