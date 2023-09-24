using BloogBot;
using BloogBot.AI;
using BloogBot.Game.Objects;
using BloogBot.Models.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace AfflictionWarlockBot
{
    [Export(typeof(IBot))]
    class AfflictionWarlockBot : Bot, IBot
    {
        public string Name => "Affliction Warlock";

        public string FileName => "AfflictionWarlockBot.dll";

        bool AdditionalTargetingCriteria(WoWUnit unit) => true;

        IBotState CreateRestState(Stack<IBotState> botStates, IDependencyContainer container) =>
            new RestState(botStates, container);

        IBotState CreateMoveToTargetState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) =>
            new MoveToTargetState(botStates, container, target);

        public IDependencyContainer GetDependencyContainer(BotSettings botSettings, InstanceUpdate probe) =>
            new DependencyContainer(
                AdditionalTargetingCriteria,
                CreateRestState,
                CreateMoveToTargetState,
                botSettings,
                probe);

        public void Test(IDependencyContainer container)
        {
        }

        public void Travel(IDependencyContainer container, bool reverseTravelPath, Action callback)
        {
            throw new NotImplementedException();
        }
    }
}
