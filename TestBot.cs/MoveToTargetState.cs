using BloogBot.AI;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace TestBot
{
    class MoveToTargetState : IBotTask
    {
        readonly Stack<IBotTask> botTasks;

        internal MoveToTargetState(Stack<IBotTask> botTasks, IClassContainer container, WoWUnit target)
        {
            this.botTasks = botTasks;
        }

        public void Update()
        {
            botTasks.Pop();
        }
    }
}
