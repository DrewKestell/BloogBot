using WoWActivityMember.Tasks;
using WoWActivityMember.Models;
using System.ComponentModel.Composition;
using MageFire.Tasks;

namespace MageFire
{
    [Export(typeof(IBot))]
    class MageArcane : IBot
    {
        public string Name => "Fire Mage";

        public string FileName => "MageFire.dll";

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
