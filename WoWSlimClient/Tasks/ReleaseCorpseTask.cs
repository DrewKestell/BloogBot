namespace WoWSlimClient.Tasks.SharedStates
{
    public class ReleaseCorpseTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        public void Update()
        {
        }
    }
}
