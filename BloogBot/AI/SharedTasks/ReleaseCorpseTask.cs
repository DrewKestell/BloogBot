using BloogBot.Game;
using System.Collections.Generic;

namespace BloogBot.AI.SharedStates
{
    public class ReleaseCorpseTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;

        public ReleaseCorpseTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.container = container;
            this.botTasks = botTasks;
        }

        public void Update()
        {
            if (ObjectManager.Player.Health > 0)
            {
                botTasks.Pop();
                return;
            }
            if (Wait.For("StartReleaseCorpseStateDelay", 1000))
            {
                if (!ObjectManager.Player.InGhostForm)
                    ObjectManager.Player.ReleaseCorpse();
                else
                {
                    if (Wait.For("LeaveReleaseCorpseStateDelay", 2000))
                    {
                        botTasks.Pop();
                        return;
                    }
                }
            }
        }
    }
}
