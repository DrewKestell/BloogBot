using BotCommLayer.Clients;
using Communication;
using PathfindingService.Client;

namespace BotRunner.Interfaces
{
    public interface IClassContainer
    {
        public string Name { get; }
        public ActivityMemberState State { get; }
        public WoWDatabaseClient WoWDatabaseClient { get; }
        public PathfindingClient PathfindingClient { get; }
        Func<IBotContext, IBotTask> CreateRestTask { get; }
        Func<IBotContext, IBotTask> CreateBuffTask { get; }
        Func<IBotContext, IBotTask> CreatePullTargetTask { get; }
        Func<IBotContext, IBotTask> CreatePvERotationTask { get; }
        Func<IBotContext, IBotTask> CreatePvPRotationTask { get; }
    }
}
