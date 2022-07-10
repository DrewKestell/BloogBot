using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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

                if (player != null)
                {
                    Console.WriteLine($"Pointer: {player.Pointer.ToString("X")}");
                    Console.WriteLine($"Position: {player.Position}");
                    Console.WriteLine($"Facing: {player.Facing}");
                    Console.WriteLine($"MapId: {ObjectManager.MapId}");

                    var start = player.Position;
                    var end = new Position(5807.83f, 587.83f, 652.38f);

                    var nextPoint = Navigation.GetNextWaypoint(ObjectManager.MapId, start, end, false);

                    player.StopMovement(ControlBits.Front);
                    player.MoveToward(nextPoint);
                }
            });
        }
    }
}

