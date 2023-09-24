using BloogBot;
using BloogBot.AI;
using BloogBot.Game.Objects;
using BloogBot.Models.Dto;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace BalanceDruidBot
{
    // NOTES:
    //  - Make sure you put 2/2 points into the Nature's Reach talent as soon as possible. This profile assumes you'll have the increased range.
    [Export(typeof(IBot))]
    class BalanceDruidBot : Bot, IBot
    {
        public string Name => "Balance Druid";

        public string FileName => "BalanceDruidBot.dll";

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
    }
}
