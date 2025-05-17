using GameData.Core.Frames;

namespace GameData.Core.Models
{
    public class QuestSlot
    {
        public uint QuestId { get; set; }
        public byte[] QuestCounters { get; set; } = [4];
        public uint QuestState { get; set; }
        public uint QuestTime { get; set; }
    }

    /// <summary>
    ///     An object representing an quest
    /// </summary>
    public abstract class QuestLogEntry
    {
        /// <summary>
        ///     The quest ID
        /// </summary>
        public readonly int Id;

        /// <summary>
        ///     The state of the quest (Completed, failed, In progress)
        /// </summary>
        public readonly QuestState State;

        /// <summary>
        ///     The rewards of the quest
        /// </summary>
        public IReadOnlyList<QuestChoiceReward> ChoiceRewards = [];

        internal int LastUpdate;

        /// <summary>
        ///     Name of the Quest
        /// </summary>
        public string Name { get; } = string.Empty;

        /// <summary>
        ///     The objectives
        /// </summary>
        public IReadOnlyList<QuestObjective> Objectives = [];

        internal int RealQuestIndex { get; }
    }

    /// <summary>
    ///     Representing an objective of a quest (Kill 8/8 boars etc.)
    /// </summary>
    public abstract class QuestObjective
    {
        /// <summary>
        ///     Is the objective done?
        /// </summary>
        public readonly bool IsDone;

        /// <summary>
        ///     The ID of the item to gather / unit to kill
        /// </summary>
        public readonly int ObjectId;

        /// <summary>
        ///     The number of objects required
        /// </summary>
        public readonly int ObjectsRequired;

        /// <summary>
        ///     How many objects we already killed / collected
        /// </summary>
        public readonly int Progress;

        /// <summary>
        ///     The objective-type
        /// </summary>
        public readonly QuestObjectiveTypes Type;
    }

    public enum QuestSlotOffsets
    {
        QUEST_ID_OFFSET = 0,
        QUEST_COUNT_STATE_OFFSET = 1, // including counters 6bits+6bits+6bits+6bits + state 8bits
        QUEST_TIME_OFFSET = 2
    }

    /// <summary>
    ///     Quest-objective types: Kill, Collect or Event
    /// </summary>
    public enum QuestObjectiveTypes : byte
    {
        Kill = 1,
        Collect = 2,
        Event = 3
    }

    /// <summary>
    ///     The possible states of an accepted quest
    /// </summary>
    public enum QuestState
    {
        Completed = 1,
        InProgress = 0,
        Failed = -1
    }
}
