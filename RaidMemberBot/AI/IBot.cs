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

        IBotTask CreateMoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target);

        IBotTask CreateBuffTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> partyMembers);

        IBotTask CreateOffensiveRotationTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> targets);

        IBotTask CreateDefensiveRotationTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> partyMembers);
    }
}
