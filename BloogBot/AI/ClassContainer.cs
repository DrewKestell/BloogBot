using BloogBot.Game.Objects;
using BloogBot.Models.Dto;
using System;
using System.Collections.Generic;

namespace BloogBot.AI
{
    public class ClassContainer : IClassContainer
    {
        public ClassContainer(
            Func<IClassContainer, Stack<IBotTask>, IBotTask> createRestState,
            Func<IClassContainer, Stack<IBotTask>, WoWUnit, IBotTask> createMoveToTargetState,
            Func<IClassContainer, Stack<IBotTask>, List<WoWUnit>, IBotTask> createBuffRotationTask,
            Func<IClassContainer, Stack<IBotTask>, List<WoWUnit>, IBotTask> createOffensiveRotationTask,
            Func<IClassContainer, Stack<IBotTask>, List<WoWUnit>, IBotTask> createDefensiveRotationTask,
            CharacterState probe)
        {
            CreateRestTask = createRestState;
            CreateMoveToAttackTargetTask = createMoveToTargetState;
            CreateBuffTask = createBuffRotationTask;
            CreateOffensiveRotationTask = createOffensiveRotationTask;
            CreateDefensiveRotationTask= createDefensiveRotationTask;
            Probe = probe;
        }

        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreateRestTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, WoWUnit, IBotTask> CreateMoveToAttackTargetTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, List<WoWUnit>, IBotTask> CreateBuffTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, List<WoWUnit>, IBotTask> CreateOffensiveRotationTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, List<WoWUnit>, IBotTask> CreateDefensiveRotationTask { get; }

        public CharacterState Probe { get; }
    }
}
