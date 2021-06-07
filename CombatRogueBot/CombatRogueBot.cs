using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace CombatRogueBot
{
    [Export(typeof(IBot))]
    
    class CombatRogueBot : Bot, IBot
    {
        public string Name => "Combat Rogue";

        public string FileName => "CombatRogueBot.dll";

        bool AdditionalTargetingCriteria(WoWUnit u) =>
            !ObjectManager.Units.Any(o =>
                o.Level > ObjectManager.Player.Level - 4 &&
                (o.UnitReaction == UnitReaction.Hated || o.UnitReaction == UnitReaction.Hostile) &&
                o.Guid != ObjectManager.Player.Guid &&
                o.Guid != u.Guid &&
                o.Position.DistanceTo(u.Position) < 20
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
