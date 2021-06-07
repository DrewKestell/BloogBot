using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;

namespace EnhancementShamanBot
{
    [Export(typeof(IBot))]
    class EnhancementShamanBot : Bot, IBot
    {
        public string Name => "Enhancement Shaman";

        public string FileName => "EnhancementShamanBot.dll";

        bool AdditionalTargetingCriteria(WoWUnit unit) => true;

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

        public void Test(IDependencyContainer container)
        {
            ThreadSynchronizer.RunOnMainThread(() =>
            {
                ObjectManager.Player.Jump();
            });
        }
    }
}
