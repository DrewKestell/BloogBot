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
            Func<IClassContainer, Stack<IBotTask>, IBotTask> createBuffRotationTask,
            Func<IClassContainer, Stack<IBotTask>, WoWUnit, IBotTask> createPullTargetTask,
            Func<IClassContainer, Stack<IBotTask>, IBotTask> createPvERotationTask,
            Func<IClassContainer, Stack<IBotTask>, IBotTask> createPvPRotationTask,
            CharacterState probe)
        {
            Name = name;
            CreateRestTask = createRestState;
            CreatePullTargetTask = createPullTargetTask;
            CreateBuffTask = createBuffRotationTask;
            CreatePvERotationTask = createPvERotationTask;
            CreatePvPRotationTask= createPvPRotationTask;
            Probe = probe;
            Console.WriteLine($"CLASS CONTAINER: {Name}");
        }

        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreateRestTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreateBuffTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, WoWUnit, IBotTask> CreatePullTargetTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePvERotationTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePvPRotationTask { get; }

        public CharacterState Probe { get; }
    }
}
