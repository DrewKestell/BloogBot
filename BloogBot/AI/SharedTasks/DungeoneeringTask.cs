using BloogBot.Game;
using System.Collections.Generic;

namespace BloogBot.AI.SharedStates
{
    public class DungeoneeringTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly int mapId;
        readonly Position start;

        string accountName;
        int characterSlot;

        public DungeoneeringTask(IClassContainer container, Stack<IBotTask> botTasks,int mapId, Position start)
        {
            this.botTasks = botTasks;
            this.container = container;
            this.mapId = mapId;
            this.start = start;
        }
        public void Update()
        {
            botTasks.Pop();
            botTasks.Push(new MoveToPositionTask(container, botTasks, start));
        }
    }
}
