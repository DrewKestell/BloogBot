using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    public abstract class BotTask(IBotContext botContext)
    {
        public readonly IBotContext BotContext = botContext;
        public readonly IObjectManager ObjectManager = botContext.ObjectManager;
        public readonly IWoWEventHandler EventHandler = botContext.EventHandler;
        public readonly IClassContainer Container =botContext.Container;
        public readonly Stack<IBotTask> BotTasks = botContext.BotTasks;
    }
}
