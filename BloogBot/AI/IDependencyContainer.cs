using BloogBot.Game.Objects;
using BloogBot.Models.Dto;
using System;
using System.Collections.Generic;

namespace BloogBot.AI
{
    public interface IDependencyContainer
    {
        Func<Stack<IBotState>, IDependencyContainer, IBotState> CreateRestState { get; }

        Func<Stack<IBotState>, IDependencyContainer, WoWUnit, IBotState> CreateMoveToTargetState { get; }

        BotSettings BotSettings { get; }

        CharacterState Probe { get; }

        WoWUnit FindClosestTarget();

        WoWUnit FindThreat();

        bool RunningErrands { get; set; }

        string AccountName { get; set; }
    }
}
