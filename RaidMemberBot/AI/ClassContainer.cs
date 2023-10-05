using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;

namespace RaidMemberBot.AI
{
    public class ClassContainer : IClassContainer
    {
        public string Name { get; set; }
        public ClassContainer(string name,
            Func<IClassContainer, Stack<IBotTask>, IBotTask> createRestState,
            Func<IClassContainer, Stack<IBotTask>, WoWUnit, IBotTask> createMoveToTargetState,
            Func<IClassContainer, Stack<IBotTask>, List<WoWUnit>, IBotTask> createBuffRotationTask,
            Func<IClassContainer, Stack<IBotTask>, List<WoWUnit>, IBotTask> createOffensiveRotationTask,
            Func<IClassContainer, Stack<IBotTask>, List<WoWUnit>, IBotTask> createDefensiveRotationTask,
            CharacterState probe)
        {
            Name = name;
            CreateRestTask = createRestState;
            CreateMoveToAttackTargetTask = createMoveToTargetState;
            CreateBuffTask = createBuffRotationTask;
            CreateOffensiveRotationTask = createOffensiveRotationTask;
            CreateDefensiveRotationTask= createDefensiveRotationTask;
            Probe = probe;
            Console.WriteLine($"[ClassContainer] {Name}\n");
        }

        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreateRestTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, WoWUnit, IBotTask> CreateMoveToAttackTargetTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, List<WoWUnit>, IBotTask> CreateBuffTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, List<WoWUnit>, IBotTask> CreateOffensiveRotationTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, List<WoWUnit>, IBotTask> CreateDefensiveRotationTask { get; }

        public CharacterState Probe { get; }
    }
}
