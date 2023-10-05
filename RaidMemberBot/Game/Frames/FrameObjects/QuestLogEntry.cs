using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Game.Frames.FrameObjects
{
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
        public IReadOnlyList<QuestChoiceReward> ChoiceRewards;

        internal int LastUpdate;

        /// <summary>
        ///     Name of the Quest
        /// </summary>
        public string Name;

        /// <summary>
        ///     The objectives
        /// </summary>
        public IReadOnlyList<QuestObjective> Objectives;

        internal int RealQuestIndex { get; private set; }

        internal QuestLogEntry(int parId, string questName, QuestState parState, QuestObjective[] parObjectives,
            QuestChoiceReward[] parChoiceRewards, int realQuestIndex)
        {
            Id = parId;
            State = parState;
            Objectives = parObjectives;
            ChoiceRewards = parChoiceRewards;
            Name = questName;
            RealQuestIndex = realQuestIndex;
        }

        public override string ToString()
        {
            string res = $"############\nQuest: {Id}\nState: {State}\nObjectives:";
            if (Objectives.Count == 0)
                res += "\n";
            foreach (var x in Objectives)
                res += $"\n\tType: {x.Type}" +
                       $"\n\tInvolved Id: {x.ObjectId}" +
                       $"\n\tRequired: {x.ObjectsRequired}" +
                       $"\n\tGot: {x.Progress}" +
                       $"\n\tIs done: {x.IsDone}\n";
            res += "############";
            return res;
        }
    }
}
