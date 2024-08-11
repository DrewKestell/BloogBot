using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    public class AddPartyMemberTask(IBotContext botContext, string characterName) : BotTask(botContext), IBotTask
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
            else if (!hasInvited)
            {
                if (ObjectManager.PartyMembers.Count() > 4 && !Container.State.InRaid)
                {
                    ObjectManager.ConvertToRaid();
                }
                ObjectManager.InviteToGroup(characterName);
                hasInvited = true;
            }
        }
    }
}
