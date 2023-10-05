using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
{
    public class LogoutTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;

        public LogoutTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
        }
        public void Update()
        {
            botTasks.Pop();
        }
    }
}
