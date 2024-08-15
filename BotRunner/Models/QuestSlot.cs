namespace BotRunner.Models
{
    public class QuestSlot
    {
        public uint QuestId { get; set; }
        public byte[] QuestCounters { get; set; } = [4];
        public uint QuestState { get; set; }
        public uint QuestTime { get; set; }
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
