using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Diagnostics;

namespace RaidMemberBot.AI.SharedTasks
{
    public class WarsongGultchTask : BotTask, IBotTask
    {
        public WarsongGultchTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary) { }

        public void Update()
        {

        }
    }
}
