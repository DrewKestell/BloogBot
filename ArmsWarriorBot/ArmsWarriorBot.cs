using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace ArmsWarriorBot
{
    [Export(typeof(IBot))]
    class ArmsWarriorBot : Bot, IBot
    {
        public string Name => "Arms Warrior";

        public string FileName => "ArmsWarriorBot.dll";

        // omit any targets that bring the player too close to another threat while moving to that target
        bool AdditionalTargetingCriteria(WoWUnit u) =>
            !ObjectManager.Units.Any(o =>
                o.Level > ObjectManager.Player.Level - 3 &&
                (o.UnitReaction == UnitReaction.Hated || o.UnitReaction == UnitReaction.Hostile) &&
                o.Guid != ObjectManager.Player.Guid &&
                o.Guid != u.Guid &&
                false &&
                Navigation.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, u.Position, false).Any(p => p.DistanceTo(o.Position) < 30)
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

        public void Test(IDependencyContainer container)
        {
            //var target = ObjectManager.Units.FirstOrDefault(u => u.Guid == ObjectManager.Player.TargetGuid);

            //if (target != null)
            //{
            //    Console.WriteLine(target.CanBeLooted);
            //}

            ThreadSynchronizer.RunOnMainThread(() =>
            {
                var results = Functions.LuaCallWithResult($"{{0}}, {{1}}, {{2}}, {{3}} = GetGossipOptions()");
                if (results.Length > 0)
                {
                    Console.WriteLine(results[0]);
                    Console.WriteLine(results[1]);
                    Console.WriteLine(results[2]);
                    Console.WriteLine(results[3]);
                }
            });
        }
    }
}
