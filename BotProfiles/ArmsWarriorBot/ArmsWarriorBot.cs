using RaidMemberBot.AI;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace ArmsWarriorBot
{
    [Export(typeof(IBot))]
    class ArmsWarriorBot : IBot
    {
        public string Name => "Arms Warrior";

        public string FileName => "ArmsWarriorBot.dll";

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
