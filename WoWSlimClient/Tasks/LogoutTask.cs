namespace WoWSlimClient.Tasks.SharedStates
{
    public class LogoutTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
