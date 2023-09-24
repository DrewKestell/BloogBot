using BloogBot;
using BloogBot.AI;
using BloogBot.Game.Objects;
using BloogBot.Models.Dto;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace ElementalShamanBot
{
    [Export(typeof(IBot))]
    class ElementalShamanBot : Bot, IBot
    {
        public string Name => "Elemental Shaman";

        public string FileName => "ElementalShamanBot.dll";

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

        public void Test(IDependencyContainer container) { }
    }
}
