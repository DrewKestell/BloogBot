using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Game.Frames.FrameObjects
{
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

        internal QuestObjective(QuestObjectiveTypes parType, int parObjectId,
            int parObjectsRequired, int parProgress, bool parIsDone)
        {
            Type = parType;
            ObjectId = parObjectId;
            ObjectsRequired = parObjectsRequired;
            Progress = parProgress;
            IsDone = parIsDone;
        }
    }
}
