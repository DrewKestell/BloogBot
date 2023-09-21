using BloogBot.Game;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static BloogBot.AI.QuestHelper;

namespace BloogBot.Models
{
    public class QuestTask
    {
        public int QuestId { get; set; }
        public string Name { get; set; }
        public Item RewardItem1 { get; set; }
        public Item RewardItem2 { get; set; }
        public Item RewardItem3 { get; set; }
        public Item RewardItem4 { get; set; }
        public Item RewardItem5 { get; set; }
        public Item RewardItem6 { get; set; }
        public Npc TurnInNpc { get; set; }
        public List<QuestObjective> QuestObjectives { get; } = new List<QuestObjective>();

        public virtual bool IsComplete()
        {
            PlayerQuest playerQuest = GetQuestsFromQuestLog().Where(x => x.ID == QuestId).First();

            if (playerQuest.State == PlayerQuest.StateFlag.Complete)
            {
                return true;
            }

            foreach (QuestObjective obj in QuestObjectives)
            {
                if (!obj.IsComplete())
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("QuestId: " + QuestId + ", Name: " + Name + ", TurnInNpc: " + JsonConvert.SerializeObject(TurnInNpc) + ", QuestObjectives.Count: " + QuestObjectives.Count);
            if (QuestObjectives.Count > 0)
            {
                stringBuilder.Append(", QuestObjectives[0].TargetId: " + QuestObjectives[0].TargetCreatureId + ", QuestObjectives[0].HotSpots.Count: " + QuestObjectives[0].HotSpots.Count);
            }
            return stringBuilder.ToString();
        }
    }
    public class QuestObjective
    {
        public int QuestId { get; set; }
        public int Index { get; set; }
        public ulong TargetCreatureId { get; set; }
        public ulong TargetGameObjectId { get; set; }
        public int TargetsNeeded { get; set; }
        public ulong ReqItemId { get; set; }
        public int ReqItemQty { get; set; }
        public int UsableItemId { get; set; }
        public int ConsumableItemId { get; set; }
        public List<Position> HotSpots { get; } = new List<Position>();
        public bool IsComplete()
        {
            if (TargetsNeeded > 0)
            {
                return IsObjectiveComplete(QuestId, Index, TargetsNeeded);
            }
            else if (ReqItemQty > 0)
            {
                return IsObjectiveComplete(QuestId, Index, ReqItemQty);
            }
            return true;
        }
        public override string ToString()
        {
            return "QuestId: " + QuestId + ", Index: " + Index + ", TargetId: " + TargetCreatureId + ", GameObjectId: " + TargetGameObjectId + ", UsableItemId: " + UsableItemId + ", ConsumableItemId: " + ConsumableItemId + ", HotSpots: " + HotSpots.Count;
        }
    }
    public class QuestCompletion
    {
        public string Name { get; set; }
        public int QuestId { get; set; }
    }
}
