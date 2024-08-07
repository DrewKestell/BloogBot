using System.ComponentModel.Composition;
using BotRunner;
using BotRunner.Interfaces;
using Communication;
using PriestShadow.Tasks;

namespace PriestShadow
{
    [Export(typeof(IBot))]
    internal class PriestShadow : IBot
    {
        public string Name => "Shadow Priest";

        public string FileName => "PriestShadow.dll";

        public IClassContainer GetClassContainer(ActivityMemberState probe) =>
            new ClassContainer(
                Name,
                CreateRestTask,
                CreateBuffTask,
                CreateMoveToTargetTask,
                CreatePvERotationTask,
                CreatePvPRotationTask,
                probe);

        public IBotTask CreateRestTask(IBotContext botContext) =>
            new RestTask(botContext);

        public IBotTask CreateMoveToTargetTask(IBotContext botContext) =>
            new PullTargetTask(botContext);

        public IBotTask CreateBuffTask(IBotContext botContext) =>
            new BuffTask(botContext);

        public IBotTask CreatePvERotationTask(IBotContext botContext) =>
            new PvERotationTask(botContext);

        public IBotTask CreatePvPRotationTask(IBotContext botContext) =>
            new PvERotationTask(botContext);
    }
}
