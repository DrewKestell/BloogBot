using BloogBot;
using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
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
                //var tile = Navigation.GetTileCoords(ObjectManager.MapId, ObjectManager.Player.Position);

                //Console.WriteLine($"MapId: {ObjectManager.MapId} - {tile.X}, {tile.Y}");
                var end = new Position(-2009.88f, -447.54f, -12.57f);

                var botStates = new Stack<IBotState>();
                var mtps = new MoveToPositionState(botStates, container, end);
                botStates.Push(mtps);

                StartInternal(botStates, container);
            });
        }

        async void StartInternal(Stack<IBotState> botStates, IDependencyContainer container)
        {
            while (true)
            {
                try
                {
                    ThreadSynchronizer.RunOnMainThread(() =>
                    {
                        var player = ObjectManager.Player;

                        player.AntiAfk();

                        if (botStates.Count() == 0)
                        {
                            Stop();
                            return;
                        }
                        if (botStates.Count > 0)
                        {
                            container.Probe.CurrentState = botStates.Peek()?.GetType().Name;
                            botStates.Peek()?.Update();
                        }
                    });

                    await Task.Delay(25);
                }
                catch (Exception e)
                {
                    Logger.Log(e + "\n");
                }
            }
        }
    }
}

