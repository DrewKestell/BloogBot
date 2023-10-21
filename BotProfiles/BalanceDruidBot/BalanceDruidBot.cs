using RaidMemberBot.AI;
using RaidMemberBot.Models.Dto;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace BalanceDruidBot
{
    // NOTES:
    //  - Make sure you put 2/2 points into the Nature's Reach talent as soon as possible. This profile assumes you'll have the increased range.
    [Export(typeof(IBot))]
    class BalanceDruidBot : IBot
    {
        public string Name => "Balance Druid";

        public string FileName => "BalanceDruidBot.dll";

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

        public IBotTask CreateBuffTask(IClassContainer container, Stack<IBotTask> botTasks) =>
            new BuffTask(container, botTasks);

        public IBotTask CreateMoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks) =>
            new PullTargetTask(container, botTasks);

        public IBotTask CreatePvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) =>
            new PvERotationTask(container, botTasks);

        public IBotTask CreatePvPRotationTask(IClassContainer container, Stack<IBotTask> botTasks) =>
            new PvERotationTask(container, botTasks);
    }
}
