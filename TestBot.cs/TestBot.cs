using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Frames;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace TestBot
{
    [Export(typeof(IBot))]
    unsafe class TestBot : Bot, IBot
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
                var target = ObjectManager.Players.FirstOrDefault(u => u.Guid == ObjectManager.Player.TargetGuid);

                if (target != null)
                    Console.WriteLine(ObjectManager.Player.InLosWith(target.Position));
            });
        }
    }
}

