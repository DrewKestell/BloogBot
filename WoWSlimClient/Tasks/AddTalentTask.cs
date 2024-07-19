namespace WoWSlimClient.Tasks.SharedStates
{
    public class AddTalentTask(IClassContainer container, Stack<IBotTask> botTasks, int spellId) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private readonly int spellId = spellId;

        public void Update()
        {
           
        }
    }
}
