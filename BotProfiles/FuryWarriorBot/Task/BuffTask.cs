using RaidMemberBot.AI;
using System.Collections.Generic;

namespace FuryWarriorBot
{
    class BuffTask : BotTask, IBotTask
    {
        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
