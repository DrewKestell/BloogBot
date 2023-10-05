using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
{
    public class DungeoneeringTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly int mapId;
        readonly Location start;

        string accountName;
        int characterSlot;

        public DungeoneeringTask(IClassContainer container, Stack<IBotTask> botTasks,int mapId, Location start)
        {
            this.botTasks = botTasks;
            this.container = container;
            this.mapId = mapId;
            this.start = start;
        }
        public void Update()
        {
            botTasks.Pop();
            botTasks.Push(new MoveToLocationTask(container, botTasks, start));
        }
    }
}
