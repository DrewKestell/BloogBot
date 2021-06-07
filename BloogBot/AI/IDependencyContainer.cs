using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;

namespace BloogBot.AI
{
    public interface IDependencyContainer
    {
        Func<Stack<IBotState>, IDependencyContainer, IBotState> CreateRestState { get; }

        Func<Stack<IBotState>, IDependencyContainer, WoWUnit, IBotState> CreateMoveToTargetState { get; }

        Func<Stack<IBotState>, IDependencyContainer, WoWUnit, WoWPlayer, IBotState> CreatePowerlevelCombatState { get; }

        BotSettings BotSettings { get; }

        Probe Probe { get; }

        IEnumerable<Hotspot> Hotspots { get; }

        WoWUnit FindClosestTarget();

        WoWUnit FindThreat();

        Hotspot GetCurrentHotspot();

        void CheckForTravelPath(Stack<IBotState> botStates, bool reverse, bool needsToRest = true);

        bool RunningErrands { get; set; }

        bool UpdatePlayerTrackers();

        bool DisableTeleportChecker { get; set; }
    }
}
