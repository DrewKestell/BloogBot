using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace TestBot
{
    [Export(typeof(IBot))]
    class TestBot : Bot, IBot
    {
        public string Name => "Tester";

        public string FileName => "TestBot.dll";

        bool AdditionalTargetingCriteria(WoWUnit u) => true;

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
                var player = ObjectManager.Player;

                var targetGuid = ObjectManager.Player.TargetGuid;
                var target = ObjectManager.Units.FirstOrDefault(u => u.Guid == targetGuid);
                if (target != null)
                {
                    var inLos = player.InLosWith(target.Position);
                    Console.WriteLine(inLos);
                }
                else
                {
                    Console.WriteLine("Target is null.");
                }
            });
        }
    }
}

