using MaNGOSDBDomain.Client;
using WoWSlimClient.Models;

namespace WoWSlimClient.Tasks
{
    public interface IClassContainer
    {
        public string Name { get; }
        public ActivityMemberState State { get; }
        public MaNGOSDBClient MaNGOSDBClient { get; }
        Func<IClassContainer, Stack<IBotTask>, IBotTask> CreateRestTask { get; }
        Func<IClassContainer, Stack<IBotTask>, IBotTask> CreateBuffTask { get; }
        Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePullTargetTask { get; }
        Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePvERotationTask { get; }
        Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePvPRotationTask { get; }
    }
}
