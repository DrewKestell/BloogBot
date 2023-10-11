using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace RaidMemberBot.AI
{
    public interface IBot
    {
        string Name { get; }

        string FileName { get; }

        IClassContainer GetClassContainer(CharacterState probe);

        IBotTask CreateRestTask(IClassContainer container, Stack<IBotTask> botTasks);

        IBotTask CreateBuffTask(IClassContainer container, Stack<IBotTask> botTasks);

        IBotTask CreateMoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target);

        IBotTask CreatePvERotationTask(IClassContainer container, Stack<IBotTask> botTasks);

        IBotTask CreatePvPRotationTask(IClassContainer container, Stack<IBotTask> botTasks);
    }
}
