using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace ProtectionWarriorBot
{
    class BuffTask : IBotTask
    {

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            botTasks.Pop();
        }
    }
}
