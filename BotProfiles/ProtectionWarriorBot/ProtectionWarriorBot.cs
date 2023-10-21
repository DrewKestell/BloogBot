using RaidMemberBot.AI;
using RaidMemberBot.Models.Dto;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace ProtectionWarriorBot
{
    [Export(typeof(IBot))]
    class ProtectionWarriorBot : IBot
    {
        public string Name => "Protection Warrior";

        public string FileName => "ProtectionWarriorBot.dll";

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
