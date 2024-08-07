using System.ComponentModel.Composition;
using BotRunner;
using BotRunner.Interfaces;
using Communication;
using HunterSurvival.Tasks;

namespace HunterSurvival
{
    [Export(typeof(IBot))]
    internal class HunterSurvival : IBot
    {
        public string Name => "Survival Hunter";

        public string FileName => "HunterSurvival.dll";

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
