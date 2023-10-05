using RaidMemberBot.AI;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
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
                CreateMoveToTargetTask,
                CreateBuffTask,
                CreateOffensiveRotationTask,
                CreateDefensiveRotationTask,
                probe);

        public IBotTask CreateRestTask(IClassContainer container, Stack<IBotTask> botTasks) =>
            new RestTask(container, botTasks);

        public IBotTask CreateMoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target) =>
            new MoveToTargetTask(container, botTasks, target);

        public IBotTask CreateBuffTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> partyMembers) =>
            new BuffTask(container, botTasks, partyMembers);

        public IBotTask CreateOffensiveRotationTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> targets) =>
            new CombatTask(container, botTasks, targets);

        public IBotTask CreateDefensiveRotationTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> partyMembers) =>
            new CombatTask(container, botTasks, partyMembers);
    }
}
