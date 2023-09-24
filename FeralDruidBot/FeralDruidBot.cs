using BloogBot;
using BloogBot.AI;
using BloogBot.Game.Objects;
using BloogBot.Models.Dto;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FeralDruidBot
{
    [Export(typeof(IBot))]
    class FeralDruidBot : Bot, IBot
    {
        public string Name => "Feral Druid";

        public string FileName => "FeralDruidBot.dll";

        bool AdditionalTargetingCriteria(WoWUnit unit) => true;

        IBotState CreateRestState(Stack<IBotState> botStates, IDependencyContainer container) =>
            new RestState(botStates, container);

        IBotState CreateMoveToTargetState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) =>
            new MoveToTargetState(botStates, container, target);

        IBotState CreatePowerlevelCombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target, WoWPlayer powerlevelTarget) =>
            new PowerlevelCombatState(botStates, container, target, powerlevelTarget);

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
