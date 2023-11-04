using RaidMemberBot.Constants;
using RaidMemberBot.ExtensionMethods;
using RaidMemberBot.Game.Frames.FrameObjects;
using RaidMemberBot.Mem;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;
using ItemCacheEntry = RaidMemberBot.Constants.ItemCacheEntry;

namespace RaidMemberBot.Game.Statics
{
    /// <summary>
    ///     Represents a Questlog
    /// </summary>
    public sealed class QuestLog
    {
        private static readonly object lockObject = new object();

        private static readonly Lazy<QuestLog> _instance =
            new Lazy<QuestLog>(() => new QuestLog());

        private volatile Dictionary<int, QuestLogEntry> _questDic = new Dictionary<int, QuestLogEntry>();
        private volatile List<QuestLogEntry> _quests = new List<QuestLogEntry>();

        /// <summary>
        ///     Access to the characters Questlog
        /// </summary>
        /// <value>
        ///     The instance.
        /// </value>
        public static QuestLog Instance
        {
            get
            {
                lock (lockObject)
                {
                    return _instance.Value;
                }
            }
        }

        /// <summary>
        ///     A readonly list with all active quests of the toon
        /// </summary>
        public IReadOnlyList<QuestLogEntry> Quests => _quests.AsReadOnly();

        /// <summary>
        /// Abandons the quest by Id
        /// </summary>
        /// <param name="questId"></param>
        public void AbandonQuest(int questId)
        {
            QuestLogEntry quest = Quests.FirstOrDefault(x => x.Id == questId);
            if (quest == null) return;
            Functions.AbandonQuest(quest.RealQuestIndex);
        }

        internal void UpdateQuestLog()
        {
            //var totalQuests = 0x00BC6F24.ReadAs<int>();
            int totalEntrys = 0xBB7478.ReadAs<int>();
            int updateTime = Environment.TickCount;
            for (int i = 0; i < totalEntrys; i++)
            {
                bool isHeader = (0xBB71C8 + (i << 4)).ReadAs<int>() == 1;
                if (isHeader) continue;
                int questId = (0xBB71C0 + (i << 4)).ReadAs<int>();
                IntPtr ptrToRow = ObjectManager.Instance.LookupQuestCachePtr(questId);
                if (ptrToRow == IntPtr.Zero) continue;
                string questName = ptrToRow.Add(0x9C).ReadString();
                QuestChoiceReward[] rewards = GetQuestChoiceRewards(ptrToRow);
                if (rewards == null) continue;
                int realQuestIndex = (0x00BB71C4 + (i << 4)).ReadAs<int>();
                QuestObjective[] objectives = GetQuestObjectives(ptrToRow, realQuestIndex);
                QuestState questState = QuestStateByIndex(i);

                QuestLogEntryInterface questEntry = new QuestLogEntryInterface(questId, questName, questState, objectives, rewards, realQuestIndex);
                if (!_questDic.ContainsKey(questId))
                {
                    _questDic.Add(questId, questEntry);
                    questEntry.LastUpdate = updateTime;
                }
                else
                {
                    QuestLogEntry oldEntry = _questDic[questId];
                    if (oldEntry.State != questEntry.State)
                        switch (questEntry.State)
                        {
                            case QuestState.Completed:
                                OnQuestComplete?.Invoke(questEntry, new EventArgs());
                                break;

                            case QuestState.Failed:
                                OnQuestFailed?.Invoke(questEntry, new EventArgs());
                                break;
                        }
                    for (int k = 0; k < oldEntry.Objectives.Count; k++)
                    {
                        QuestObjective oldObjective = oldEntry.Objectives[k];
                        QuestObjective newObjective = questEntry.Objectives[k];
                        if (oldObjective.Progress != newObjective.Progress)
                            OnQuestProgress?.Invoke(questEntry, new EventArgs());
                        if (oldObjective.IsDone == newObjective.IsDone) continue;
                        if (newObjective.IsDone)
                            OnQuestObjectiveComplete?.Invoke(questEntry, new EventArgs());
                    }
                    _questDic[questId] = questEntry;
                    questEntry.LastUpdate = updateTime;
                }
            }

            List<int> invalidQuests = _questDic.Where(i => i.Value.LastUpdate != updateTime).Select(i => i.Key).ToList();
            foreach (int item in invalidQuests)
                _questDic.Remove(item);
            _quests = _questDic.Values.ToList();
        }

        /// <summary>
        ///     Get a quest by ID
        /// </summary>
        /// <param name="parId">The quest ID</param>
        /// <returns>null if character didnt accept the quest. Otherwise a QuestLogEntry representing that quest</returns>
        public QuestLogEntry GetQuestById(int parId)
        {
            List<QuestLogEntry> list = _quests;
            return list.FirstOrDefault(i => i.Id == parId);
        }

        private static QuestState QuestStateByIndex(int questIndex)
        {
            int shiftedQuestIndex = questIndex << 4;
            int int1 = (shiftedQuestIndex + 0x00BB71C8).ReadAs<int>();
            int int2 = (shiftedQuestIndex + 0xBB71C4).ReadAs<int>();
            int ptr = ObjectManager.Instance.Player.ReadRelative<int>(0xE68) + 4 * 3 * int2;
            byte int4 = (ptr + 0x2f).ReadAs<byte>();
            bool failed = int1 == 0 && int2 >= 0 && int2 <= 20 && ptr != -40 && int4 == 2;
            if (failed) return QuestState.Failed;
            if (int1 == 0 && (0xBB71CC + shiftedQuestIndex).ReadAs<int>() != 0) return QuestState.Failed;
            if (Functions.CanCompleteQuest(questIndex)) return QuestState.Completed;
            return QuestState.InProgress;
        }

        private static QuestObjective[] GetQuestObjectives(IntPtr questCacheRowPtr, int realIndex)
        {
            int dynamicQuestObject = (ObjectManager.Instance.Player.Pointer + 0xe68).ReadAs<int>() + 12 * realIndex +
                                     0x28;
            // ################################
            // ##########Event#Objective#######
            // ################################
            List<QuestObjective> objectives = new List<QuestObjective>();
            IntPtr eventObjective = questCacheRowPtr + 0x129C;
            if (eventObjective.ReadAs<int>() != 0)
            {
                byte bt = (dynamicQuestObject + 7).ReadAs<byte>();
                bool eventIsDone = (bt & 1) == 1;
                QuestObjectiveInterface tmp = new QuestObjectiveInterface(QuestObjectiveTypes.Event, 0, 0, 0, eventIsDone);
                objectives.Add(tmp);
            }
            // ################################
            // ##########Kill#Objective########
            // ################################
            int objectiveIndex = 0;
            while (objectiveIndex < 4)
            {
                IntPtr killObjective = questCacheRowPtr + 0x149C + objectiveIndex * 4;
                if (killObjective.ReadAs<int>() != 0)
                {
                    int killObjectiveProgress = ((dynamicQuestObject + 4).ReadAs<int>() >> (6 * objectiveIndex)) & 0x3F;
                    int killObjectiveId = (questCacheRowPtr + 4 * objectiveIndex + 0x149C).ReadAs<int>();
                    int killObjectiveCount = (questCacheRowPtr + 4 * objectiveIndex + 0x14AC).ReadAs<int>();
                    bool killObjectiveIsDone = killObjectiveProgress == killObjectiveCount;

                    QuestObjectiveInterface tmp = new QuestObjectiveInterface(QuestObjectiveTypes.Kill, killObjectiveId,
                        killObjectiveCount, killObjectiveProgress, killObjectiveIsDone);
                    objectives.Add(tmp);
                }
                else
                {
                    IntPtr itemObjective = questCacheRowPtr + 0x14BC + objectiveIndex * 4;
                    if (itemObjective.ReadAs<int>() != 0)
                    {
                        int itemObjectiveCount = (questCacheRowPtr + 4 * objectiveIndex + 0x14CC).ReadAs<int>();
                        int itemObjectiveId = (questCacheRowPtr + 4 * objectiveIndex + 0x14BC).ReadAs<int>();
                        int itemObjectiveProgress = Inventory.Instance.GetItemCount(itemObjectiveId);
                        bool itemObjectiveIsDone = itemObjectiveCount == itemObjectiveProgress;
                        QuestObjectiveInterface tmp = new QuestObjectiveInterface(QuestObjectiveTypes.Collect, itemObjectiveId,
                            itemObjectiveCount, itemObjectiveProgress, itemObjectiveIsDone);
                        objectives.Add(tmp);
                    }
                }
                objectiveIndex++;
            }
            // ################################
            // ######Returning#the#object######
            // ################################
            return objectives.ToArray();
        }

        /// <summary>
        ///     Occurs when all objectives of a quest are done
        ///     Sender of type QuestLogEntry
        /// </summary>
        internal event EventHandler OnQuestComplete;

        /// <summary>
        ///     Occurs when all objectives of a quest are done
        ///     Sender of type QuestLogEntry
        /// </summary>
        internal event EventHandler OnQuestObjectiveComplete;

        /// <summary>
        ///     Occurs when a quest failed
        ///     Sender of type QuestLogEntry
        /// </summary>
        internal event EventHandler OnQuestFailed;

        /// <summary>
        ///     Occurs on quest progress (required unit killed, item collected, event completed etc.)
        ///     Sender of type QuestLogEntry
        /// </summary>
        internal event EventHandler OnQuestProgress;

        private static QuestChoiceReward[] GetQuestChoiceRewards(IntPtr questCacheRowPtr)
        {
            // ################################
            // ##########Choice##Rewards#######
            // ################################
            List<QuestChoiceReward> choiceRewards = new List<QuestChoiceReward>();
            for (int i = 0; i < 6; i++)
            {
                int tmpReward = (questCacheRowPtr + 4 * i + 0x5C).ReadAs<int>();
                if ((i & 0x80000000) != 0 || tmpReward == 0) break;
                int count = (questCacheRowPtr + 4 * i + 0x74).ReadAs<int>();
                ItemCacheEntry? entry =
                    ObjectManager.Instance.LookupItemCacheEntry(tmpReward, PrivateEnums.ItemCacheLookupType.None);
                if (entry == null) return null;
                ItemCacheEntry entry2 = entry.Value;
                choiceRewards.Add(new QuestChoiceRewardInterface(tmpReward, count, ref entry2));
            }
            return choiceRewards.ToArray();
        }

        private class QuestLogEntryInterface : QuestLogEntry
        {
            internal QuestLogEntryInterface(int parQuestId, string questName, QuestState parState,
                QuestObjective[] parObjectives, QuestChoiceReward[] parChoiceRewards, int realQuestIndex)
                : base(parQuestId, questName, parState, parObjectives, parChoiceRewards, realQuestIndex)
            {
            }
        }

        private class QuestObjectiveInterface : QuestObjective
        {
            internal QuestObjectiveInterface(QuestObjectiveTypes parType, int parObjectId,
                int parObjectsRequired, int parProgress, bool parIsDone) :
                base(parType, parObjectId,
                    parObjectsRequired, parProgress, parIsDone)
            {
            }
        }

        private class QuestChoiceRewardInterface : QuestChoiceReward
        {
            internal QuestChoiceRewardInterface(int parItemId, int parItemCount, ref ItemCacheEntry parItemEntry)
                : base(parItemId, parItemCount, ref parItemEntry)
            {
            }
        }
    }
}
