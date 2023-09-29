using BloogBot.AI;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace TestBot
{
    class PowerlevelCombatState : IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly WoWUnit target;

        public PowerlevelCombatState(Stack<IBotTask> botTasks, IClassContainer container, WoWUnit target, WoWPlayer powerlevelTarget)
        {
            this.botTasks = botTasks;
            this.container = container;
            this.target = target;
        }

        public void Update()
        {
            // TODO
        }
    }
}
