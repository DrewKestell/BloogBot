using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using BloogBot.Models.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace TestBot
{
    [Export(typeof(IBot))]
    class TestBot : Bot, IBot
    {
        public string Name => "Tester";

        public string FileName => "TestBot.dll";

        bool AdditionalTargetingCriteria(WoWUnit u) => true;

        IBotTask CreateRestState(Stack<IBotTask> botTasks, IClassContainer container) =>
            new RestState(botTasks, container);

        IBotTask CreateMoveToTargetState(Stack<IBotTask> botTasks, IClassContainer container, WoWUnit target) =>
            new MoveToTargetState(botTasks, container, target);

        IBotTask CreatePowerlevelCombatState(Stack<IBotTask> botTasks, IClassContainer container, WoWUnit target, WoWPlayer powerlevelTarget) =>
            new PowerlevelCombatState(botTasks, container, target, powerlevelTarget);

        public IClassContainer GetClassContainer(BotSettings botSettings, CharacterState probe) =>
            new ClassContainer(
                AdditionalTargetingCriteria,
                CreateRestState,
                CreateMoveToTargetState,
                botSettings,
                probe);

        public void Test(IClassContainer container)
        {
            ThreadSynchronizer.RunOnMainThread(() =>
            {
                var player = ObjectManager.Player;

                var result = player.LuaCallWithResults("{0} = IsAttackAction(84)");
                if (result.Length > 0)
                {
                    Console.WriteLine(result.Length);
                    foreach (var r in result)
                    {
                        Console.WriteLine(r);
                    }
                }    
            });
        }
    }
}

