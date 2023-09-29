using BloogBot.AI;
using System.Collections.Generic;

namespace TestBot
{
    class RestState : IBotTask
    {
        readonly Stack<IBotTask> botTasks;

        public RestState(Stack<IBotTask> botTasks, IClassContainer container)
        {
            this.botTasks = botTasks;
        }

        public void Update()
        {
            botTasks.Pop();
        }
    }
}
