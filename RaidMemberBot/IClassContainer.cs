using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;

namespace RaidMemberBot.AI
{
    public interface IClassContainer
    {
        Func<IClassContainer, Stack<IBotTask>, IBotTask> CreateRestTask { get; }

        Func<IClassContainer, Stack<IBotTask>, IBotTask> CreateBuffTask { get; }

        Func<IClassContainer, Stack<IBotTask>, WoWUnit, IBotTask> CreatePullTargetTask { get; }

        Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePvERotationTask { get; }

        Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePvPRotationTask { get; }
    }
}
