using BotRunner;
using BotRunner.Interfaces;
using Communication;
using HunterMarksmanship.Tasks;
using System.ComponentModel.Composition;

namespace HunterMarksmanship
{
    [Export(typeof(IBot))]
    internal class DruidBalance : IBot
    {
        public string Name => "Marksmanship Hunter";

        public string FileName => "HunterMarksmanship.dll";

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
