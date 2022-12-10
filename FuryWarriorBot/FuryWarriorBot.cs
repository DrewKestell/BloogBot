using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FuryWarriorBot
{
    [Export(typeof(IBot))]
    class FuryWarriorBot : Bot, IBot
    {
        public string Name => "Fury Warrior";

        public string FileName => "FuryWarriorBot.dll";

        // omit any targets that bring the player too close to another threat while moving to that target
        bool AdditionalTargetingCriteria(WoWUnit u) =>
            !ObjectManager.Units.Any(o =>
                o.Level > ObjectManager.Player.Level - 4 &&
                (o.UnitReaction == UnitReaction.Hated || o.UnitReaction == UnitReaction.Hostile) &&
                o.Guid != ObjectManager.Player.Guid &&
                o.Guid != u.Guid &&
                false &&
                Navigation.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, u.Position, false).Any(p => p.DistanceTo(o.Position) < 30) &&
                u.Position.DistanceTo(o.Position) < 30
            );

        IBotState CreateRestState(Stack<IBotState> botStates, IDependencyContainer container) =>
            new RestState(botStates, container);

        IBotState CreateMoveToTargetState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) =>
            new MoveToTargetState(botStates, container, target);

        IBotState CreatePowerlevelCombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target, WoWPlayer powerlevelTarget) =>
            new PowerlevelCombatState(botStates, container, target, powerlevelTarget);

        public IDependencyContainer GetDependencyContainer(BotSettings botSettings, Probe probe, IEnumerable<Hotspot> hotspots) =>
            new DependencyContainer(
                AdditionalTargetingCriteria,
                CreateRestState,
                CreateMoveToTargetState,
                CreatePowerlevelCombatState,
                botSettings,
                probe,
                hotspots);

        public void Test(IDependencyContainer container) { }
    }
}
