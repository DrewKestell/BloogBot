using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FrostMageBot
{
    [Export(typeof(IBot))]
    class FrostMageBot : Bot, IBot
    {
        public string Name => "Frost Mage";

        public string FileName => "FrostMageBot.dll";

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
            var target = ObjectManager.Units.FirstOrDefault(u => u.Guid == ObjectManager.Player.TargetGuid);

            if (target != null)
            {
                Console.WriteLine(ObjectManager.Player.Pointer.ToString("X"));
            }
        }
    }
}
