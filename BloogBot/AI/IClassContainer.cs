using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;

namespace BloogBot.AI
{
    public interface IClassContainer
    {

        Func<IClassContainer, Stack<IBotTask>, IBotTask> CreateRestTask { get; }

        Func<IClassContainer, Stack<IBotTask>, WoWUnit, IBotTask> CreateMoveToAttackTargetTask { get; }

        Func<IClassContainer, Stack<IBotTask>, List<WoWUnit>, IBotTask> CreateBuffTask { get; }

        Func<IClassContainer, Stack<IBotTask>, List<WoWUnit>, IBotTask> CreateOffensiveRotationTask { get; }

        Func<IClassContainer, Stack<IBotTask>, List<WoWUnit>, IBotTask> CreateDefensiveRotationTask { get; }
    }
}
