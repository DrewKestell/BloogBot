using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
{
    public class LogoutTask : BotTask, IBotTask
    {
        public LogoutTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary) { }
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
