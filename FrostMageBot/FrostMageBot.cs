using BloogBot.AI;
using BloogBot.Game.Objects;
using BloogBot.Models.Dto;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FrostMageBot
{
    [Export(typeof(IBot))]
    class FrostMageBot : IBot
    {
        public string Name => "Frost Mage";

        public string FileName => "FrostMageBot.dll";

        public IClassContainer GetClassContainer(CharacterState probe) =>
            new ClassContainer(
                CreateRestTask,
                CreateMoveToTargetTask,
                CreateBuffTask,
                CreateOffensiveRotationTask,
                CreateDefensiveRotationTask,
                probe);

        public IBotTask CreateRestTask(IClassContainer container, Stack<IBotTask> botTasks) =>
            new RestTask(container, botTasks);

        public IBotTask CreateMoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target) =>
            new MoveToTargetTask(container, botTasks, target);

        public IBotTask CreateBuffTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> partyMembers) =>
            new BuffTask(container, botTasks, partyMembers);

        public IBotTask CreateOffensiveRotationTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> targets) =>
            new CombatTask(container, botTasks, targets);

        public IBotTask CreateDefensiveRotationTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> partyMembers) =>
            new CombatTask(container, botTasks, partyMembers);
    }
}
