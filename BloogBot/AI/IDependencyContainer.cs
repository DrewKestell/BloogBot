using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;

namespace BloogBot.AI
{
    public interface IDependencyContainer
    {
        Func<Stack<IBotState>, IDependencyContainer, IBotState> CreateRestState { get; }

        Func<Stack<IBotState>, IDependencyContainer, WoWUnit, IBotState> CreateMoveToTargetState { get; }

        BotSettings BotSettings { get; }

        Probe Probe { get; }

        WoWUnit FindClosestTarget();

        WoWUnit FindThreat();

        bool RunningErrands { get; set; }
    }
}
