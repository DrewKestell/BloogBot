using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace ProtectionWarriorBot
{
    class BuffTask : IBotTask
    {

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> partyMembers)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            botTasks.Pop();
        }
    }
}
