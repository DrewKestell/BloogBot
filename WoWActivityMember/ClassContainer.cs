using MaNGOSDBDomain.Client;
using WoWActivityMember.Models;

namespace WoWActivityMember.Tasks
{
    public class ClassContainer : IClassContainer
    {
        public string Name { get; }
        public CharacterState State { get; }
        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreateRestTask { get; }
        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreateBuffTask { get; }
        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePullTargetTask { get; }
        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePvERotationTask { get; }
        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePvPRotationTask { get; }
        public MaNGOSDBClient MaNGOSDBClient { get; }

        public ClassContainer(string name,
            Func<IClassContainer, Stack<IBotTask>, IBotTask> createRestState,
            Func<IClassContainer, Stack<IBotTask>, IBotTask> createBuffRotationTask,
            Func<IClassContainer, Stack<IBotTask>, IBotTask> createPullTargetTask,
            Func<IClassContainer, Stack<IBotTask>, IBotTask> createPvERotationTask,
            Func<IClassContainer, Stack<IBotTask>, IBotTask> createPvPRotationTask,
            CharacterState state)
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
