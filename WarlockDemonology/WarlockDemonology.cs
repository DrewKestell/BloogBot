using WoWActivityMember.Tasks;
using WoWActivityMember.Models;
using System.ComponentModel.Composition;
using WarlockDemonology.Tasks;

namespace WarlockDemonology
{
    [Export(typeof(IBot))]
    class WarlockDemonology : IBot
    {
        public string Name => "Demonology Warlock";

        public string FileName => "WarlockDemonology.dll";

        public IClassContainer GetClassContainer(CharacterState probe) =>
            new ClassContainer(
                Name,
                CreateRestTask,
                CreateBuffTask,
                CreateMoveToTargetTask,
                CreatePvERotationTask,
                CreatePvPRotationTask,
                probe);

        public IBotTask CreateRestTask(IClassContainer container, Stack<IBotTask> botTasks) =>
            new RestTask(container, botTasks);

        public IBotTask CreateMoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks) =>
            new PullTargetTask(container, botTasks);

        public IBotTask CreateBuffTask(IClassContainer container, Stack<IBotTask> botTasks) =>
            new BuffTask(container, botTasks);

        public IBotTask CreatePvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) =>
            new PvERotationTask(container, botTasks);

        public IBotTask CreatePvPRotationTask(IClassContainer container, Stack<IBotTask> botTasks) =>
            new PvERotationTask(container, botTasks);
    }
}
