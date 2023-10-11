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
                CreateBuffTask,
                CreateMoveToTargetTask,
                CreatePvERotationTask,
                CreatePvPRotationTask,
                probe);

        public IBotTask CreateRestTask(IClassContainer container, Stack<IBotTask> botTasks) =>
            new RestTask(container, botTasks);

        public IBotTask CreateBuffTask(IClassContainer container, Stack<IBotTask> botTasks) =>
            new BuffTask(container, botTasks);

        public IBotTask CreateMoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target) =>
            new MoveToTargetTask(container, botTasks, target);

        public IBotTask CreatePvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) =>
            new PvERotationTask(container, botTasks);

        public IBotTask CreatePvPRotationTask(IClassContainer container, Stack<IBotTask> botTasks) =>
            new PvERotationTask(container, botTasks);
    }
}
