using BotCommLayer.Clients;
using BotRunner.Interfaces;
using Communication;
using PathfindingService.Client;

namespace BotRunner
{
    public class ClassContainer : IClassContainer
    {
        public string Name { get; }
        public ActivityMemberState State { get; }
        public Func<IBotContext, IBotTask> CreateRestTask { get; }
        public Func<IBotContext, IBotTask> CreateBuffTask { get; }
        public Func<IBotContext, IBotTask> CreatePullTargetTask { get; }
        public Func<IBotContext, IBotTask> CreatePvERotationTask { get; }
        public Func<IBotContext, IBotTask> CreatePvPRotationTask { get; }
        public WoWDatabaseClient WoWDatabaseClient { get; }
        public PathfindingClient PathfindingClient { get; }

        public ClassContainer(string name,
            Func<IBotContext, IBotTask> createRestState,
            Func<IBotContext, IBotTask> createBuffRotationTask,
            Func<IBotContext, IBotTask> createPullTargetTask,
            Func<IBotContext, IBotTask> createPvERotationTask,
            Func<IBotContext, IBotTask> createPvPRotationTask,
            ActivityMemberState state)
        {
            Name = name;
            State = state;

            //MaNGOSDBClient = new MaNGOSDBClient(State.DatabasePort, IPAddress.Parse(State.DatabaseAddress));

            CreateRestTask = createRestState;
            CreatePullTargetTask = createPullTargetTask;
            CreateBuffTask = createBuffRotationTask;
            CreatePvERotationTask = createPvERotationTask;
            CreatePvPRotationTask = createPvPRotationTask;

            Console.WriteLine($"[WOW CLIENT BOT : CLASS CONTAINER] {Name}");
        }
    }
}
