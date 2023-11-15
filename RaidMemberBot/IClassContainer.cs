using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;

namespace RaidMemberBot.AI
{
    public interface IClassContainer
    {
        public string Name { get; }
        public CharacterState State { get; }
        public Position CurrentWaypoint { get; set; }
        public WoWUnit HostileTarget { get; set; }
        public WoWUnit FriendlyTarget { get; set; }
        Func<IClassContainer, Stack<IBotTask>, IBotTask> CreateRestTask { get; }
        Func<IClassContainer, Stack<IBotTask>, IBotTask> CreateBuffTask { get; }
        Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePullTargetTask { get; }
        Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePvERotationTask { get; }
        Func<IClassContainer, Stack<IBotTask>, IBotTask> CreatePvPRotationTask { get; }
    }
}
