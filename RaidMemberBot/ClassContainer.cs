using RaidMemberBot.Game.Statics;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;

namespace RaidMemberBot.AI
{
    public class ClassContainer : IClassContainer
    {
        public string Name { get; }
        public CharacterState State { get; }
        public LocalPlayer Player { get => ObjectManager.Instance.Player; }
        public Location CurrentWaypoint { get; set; }
        public WoWUnit HostileTarget { get; set; }
        public WoWUnit FriendlyTarget { get; set; }
        public ClassContainer(string name,
            Func<IClassContainer, Stack<IBotTask>, IBotTask> createRestState,
            Func<IClassContainer, Stack<IBotTask>, IBotTask> createBuffRotationTask,
            Func<IClassContainer, Stack<IBotTask>, IBotTask> createPullTargetTask,
            Func<IClassContainer, Stack<IBotTask>, IBotTask> createPvERotationTask,
            Func<IClassContainer, Stack<IBotTask>, IBotTask> createPvPRotationTask,
            CharacterState state)
        {
            Name = name;
            State = state;

            CreateRestTask = createRestState;
            CreatePullTargetTask = createPullTargetTask;
            CreateBuffTask = createBuffRotationTask;
            CreatePvERotationTask = createPvERotationTask;
            CreatePvPRotationTask = createPvPRotationTask;

            Console.WriteLine($"CLASS CONTAINER: {Name}");
        }

        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreateRestTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreateBuffTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePullTargetTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePvERotationTask { get; }

        public Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePvPRotationTask { get; }
    }
}
