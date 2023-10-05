using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
{
    public class ReleaseCorpseTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;

        public ReleaseCorpseTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.container = container;
            this.botTasks = botTasks;
        }

        public void Update()
        {
        }
    }
}
