using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
{
    public class ReleaseCorpseTask : BotTask, IBotTask
    {
        public ReleaseCorpseTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary) { }

        public void Update()
        {
        }
    }
}
