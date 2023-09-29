using BloogBot.AI;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace TestBot
{
    class CombatState : IBotTask
    {
        readonly Stack<IBotTask> botTasks;

        internal CombatState(Stack<IBotTask> botTasks, IClassContainer container, WoWUnit target)
        {
            this.botTasks = botTasks;
        }

        public void Update()
        {
            botTasks.Pop();
        }
    }
}
