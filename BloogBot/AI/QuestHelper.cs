using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using BloogBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using WoWBot.Client.Models;

namespace BloogBot.AI
{
    public static class QuestHelper
    {
        public static Position GetNextPositionHotSpot()
        {
            QuestObjective closestObjective = GrindingState.ClosestObjective;

            if (closestObjective != null)
            {
                Position currentHotSpot = GrindingState.CurrentHotSpot;

                if (currentHotSpot != null && !closestObjective.HotSpots.Contains(currentHotSpot))
                {
                    currentHotSpot = closestObjective.HotSpots.OrderBy(x => x.DistanceTo(ObjectManager.Player.Position)).ToArray()[0];
                }
                else
                {
                    if (closestObjective.HotSpots.Count > 1)
                    {
                        int index = closestObjective.HotSpots.IndexOf(currentHotSpot);
                        if (index == closestObjective.HotSpots.Count - 1)
                        {
                            closestObjective.HotSpots.Reverse();
                            currentHotSpot = closestObjective.HotSpots[0];
                        }
                        else
                        {
                            currentHotSpot = closestObjective.HotSpots[index + 1];
                        }
                    }
                    else
                    {
                        currentHotSpot = GrindingState.RemainingQuestObjectives.FindAll(x => x != closestObjective)
                                                        .SelectMany(x => x.HotSpots)
                                                        .OrderBy(spot => spot.DistanceTo(ObjectManager.Player.Position))
                                                        .ToList()[0];
                    }
                }
                GrindingState.CurrentHotSpot = currentHotSpot;
                return GrindingState.CurrentHotSpot;
            }
            else if (GrindingState.GetQuestTasks().Count > 0 && GrindingState.GetQuestTasks().All(x => x.IsComplete()))
            {
                return GrindingState.GetQuestTasks().Where(x => x.IsComplete())
                                                .OrderBy(x => Navigation.DistanceViaPath(ObjectManager.MapId, x.TurnInNpc.Position, ObjectManager.Player.Position))
                                                .ToArray()[0].TurnInNpc.Position;
            }

            return null;
        }
        public static List<WoWUnit> NearbyQuestGivers
        {
            get
            {
                return ObjectManager.Units
                                .Where(x => (x.NpcMarkerFlags & NpcMarkerFlags.YellowExclamation) == NpcMarkerFlags.YellowExclamation)
                                .OrderBy(x => Navigation.DistanceViaPath(ObjectManager.MapId, x.Position, ObjectManager.Player.Position))
                                .ToList();
            }
        }

        public static List<WoWUnit> NearbyQuestTurnIns
        {
            get
            {
                return ObjectManager.Units
                                .Where(x => (x.NpcMarkerFlags & NpcMarkerFlags.YellowQuestion) == NpcMarkerFlags.YellowQuestion)
                                .OrderBy(x => Navigation.DistanceViaPath(ObjectManager.MapId, x.Position, ObjectManager.Player.Position))
                                .ToList();
            }
        }
        public static QuestTask GetQuestTaskById(int id)
        {
            QuestTemplate questTemplate = SqliteRepository.GetQuestTemplateByID(id);
            List<int> relatedNpcIds = SqliteRepository.GetQuestRelatedNPCsByQuestId(id);

            List<Creature> relatedNpcs = new List<Creature>();

            QuestTask questTask = new QuestTask
            {
                QuestId = questTemplate.Entry,
                Name = questTemplate.Title,
            };

            for (int i = 0; i < relatedNpcIds.Count; i++)
            {
                relatedNpcs.AddRange(SqliteRepository.GetCreaturesById((ulong)relatedNpcIds[i]));
            }

            if (relatedNpcs.Count > 0)
            {
                relatedNpcs.Sort((x, y) =>
                    new Position(x.PositionX, x.PositionY, x.PositionZ).DistanceTo(ObjectManager.Player.Position)
                        .CompareTo(new Position(y.PositionX, y.PositionY, y.PositionZ).DistanceTo(ObjectManager.Player.Position)));

                CreatureTemplate creatureTemplate = SqliteRepository.GetCreatureTemplateById((ulong)relatedNpcIds[0]);

                questTask.TurnInNpc = new Npc
                {
                    NpcId = relatedNpcs[0].Id,
                    NpcName = creatureTemplate.Name,
                    Position = new Position(relatedNpcs[0].PositionX, relatedNpcs[0].PositionY, relatedNpcs[0].PositionZ)
                };
            }

            // Requires killing a mob (ReqCreatureOrGOId > 0), looting a game object (ReqCreatureOrGOId < 0), or requires an item looted from
            // a mob or a game object (not an issued by the quest giver)
            if (questTemplate.ReqCreatureOrGOId1 != 0 || (questTemplate.ReqItemId1 != 0 && questTemplate.SrcItemId != questTemplate.ReqItemId1))
            {
                questTask.QuestObjectives.Add(GetQuestObjective(1, questTemplate));
                if (questTemplate.ReqCreatureOrGOId2 != 0 || (questTemplate.ReqItemId2 != 0 && questTemplate.SrcItemId != questTemplate.ReqItemId2))
                {
                    questTask.QuestObjectives.Add(GetQuestObjective(2, questTemplate));
                    if (questTemplate.ReqCreatureOrGOId3 != 0 || (questTemplate.ReqItemId3 != 0 && questTemplate.SrcItemId != questTemplate.ReqItemId3))
                    {
                        questTask.QuestObjectives.Add(GetQuestObjective(3, questTemplate));
                        if (questTemplate.ReqCreatureOrGOId4 != 0 || (questTemplate.ReqItemId4 != 0 && questTemplate.SrcItemId != questTemplate.ReqItemId4))
                        {
                            questTask.QuestObjectives.Add(GetQuestObjective(4, questTemplate));
                        }
                    }
                }
            }
            else
            {
                questTask.QuestObjectives.Add(GetQuestObjective(1, questTemplate));

                if (questTask.QuestObjectives.Count > 0)
                {
                    questTask.QuestObjectives[0].HotSpots.Add(questTask.TurnInNpc.Position);
                }
            }

            if (questTemplate.RewChoiceItemId1 != 0)
            {
                questTask.RewardItem1 = SqliteRepository.GetItemById(questTemplate.RewChoiceItemId1);
                if (questTemplate.RewChoiceItemId2 != 0)
                {
                    questTask.RewardItem2 = SqliteRepository.GetItemById(questTemplate.RewChoiceItemId2);
                    if (questTemplate.RewChoiceItemId3 != 0)
                    {
                        questTask.RewardItem3 = SqliteRepository.GetItemById(questTemplate.RewChoiceItemId3);
                        if (questTemplate.RewChoiceItemId4 != 0)
                        {
                            questTask.RewardItem4 = SqliteRepository.GetItemById(questTemplate.RewChoiceItemId4);
                            if (questTemplate.RewChoiceItemId5 != 0)
                            {
                                questTask.RewardItem5 = SqliteRepository.GetItemById(questTemplate.RewChoiceItemId5);
                                if (questTemplate.RewChoiceItemId6 != 0)
                                {
                                    questTask.RewardItem6 = SqliteRepository.GetItemById(questTemplate.RewChoiceItemId6);
                                }
                            }
                        }
                    }
                }
            }

            return questTask;
        }

        private static QuestObjective GetQuestObjective(int objectiveIndex, QuestTemplate questTemplate)
        {
            QuestObjective questObjective = new QuestObjective()
            {
                QuestId = questTemplate.Entry,
                Index = objectiveIndex,
            };

            switch (objectiveIndex)
            {
                case 1:
                    if (questTemplate.ReqCreatureOrGOId1 > 0)
                    {
                        questObjective.TargetCreatureId = (ulong)questTemplate.ReqCreatureOrGOId1;
                    }
                    else if (questTemplate.ReqCreatureOrGOId1 < 0)
                    {
                        questObjective.TargetGameObjectId = (ulong)Math.Abs(questTemplate.ReqCreatureOrGOId1);
                    }
                    if (questTemplate.ReqCreatureOrGOCount1 != 0)
                    {
                        questObjective.TargetsNeeded = questTemplate.ReqCreatureOrGOCount1;
                    }
                    break;
                case 2:
                    if (questTemplate.ReqCreatureOrGOId2 > 0)
                    {
                        questObjective.TargetCreatureId = (ulong)questTemplate.ReqCreatureOrGOId2;
                    }
                    else if (questTemplate.ReqCreatureOrGOId2 < 0)
                    {
                        questObjective.TargetGameObjectId = (ulong)Math.Abs(questTemplate.ReqCreatureOrGOId2);
                    }
                    if (questTemplate.ReqCreatureOrGOCount2 != 0)
                    {
                        questObjective.TargetsNeeded = questTemplate.ReqCreatureOrGOCount2;
                    }
                    break;
                case 3:
                    if (questTemplate.ReqCreatureOrGOId3 > 0)
                    {
                        questObjective.TargetCreatureId = (ulong)questTemplate.ReqCreatureOrGOId3;
                    }
                    else if (questTemplate.ReqCreatureOrGOId3 < 0)
                    {
                        questObjective.TargetGameObjectId = (ulong)Math.Abs(questTemplate.ReqCreatureOrGOId3);
                    }
                    if (questTemplate.ReqCreatureOrGOCount2 != 0)
                    {
                        questObjective.TargetsNeeded = questTemplate.ReqCreatureOrGOCount2;
                    }
                    break;
                case 4:
                    if (questTemplate.ReqCreatureOrGOId4 > 0)
                    {
                        questObjective.TargetCreatureId = (ulong)questTemplate.ReqCreatureOrGOId4;
                    }
                    else if (questTemplate.ReqCreatureOrGOId4 < 0)
                    {
                        questObjective.TargetGameObjectId = (ulong)Math.Abs(questTemplate.ReqCreatureOrGOId4);
                    }
                    if (questTemplate.ReqCreatureOrGOCount4 != 0)
                    {
                        questObjective.TargetsNeeded = questTemplate.ReqCreatureOrGOCount4;
                    }
                    break;
            }


            if (questTemplate.SrcItemId != 0)
            {
                // Item is usable on the mob
                if (questObjective.TargetCreatureId > 0)
                {
                    questObjective.UsableItemId = questTemplate.SrcItemId;
                }
                // Item should be consumed
                else
                {
                    questObjective.ConsumableItemId = questTemplate.SrcItemId;
                }
            }

            //Find items that need to be looted from either mobs or game objects
            List<Creature> creatures = new List<Creature>();
            List<GameObject> gameObjects = new List<GameObject>();

            switch (objectiveIndex)
            {
                case 1:
                    if (questTemplate.ReqItemId1 != 0)
                    {
                        creatures = SqliteRepository.GetCreaturesByLootableItemId(questTemplate.ReqItemId1);
                        gameObjects = SqliteRepository.GetGameObjectByLootableItemId(questTemplate.ReqItemId1);

                        questObjective.ReqItemQty = questTemplate.ReqItemCount1;
                    }
                    else if (questObjective.TargetCreatureId != 0)
                    {
                        List<Creature> questCreatures = SqliteRepository.GetCreaturesById(questObjective.TargetCreatureId);

                        foreach (Creature creature in questCreatures)
                        {
                            questObjective.HotSpots.Add(new Position(creature.PositionX, creature.PositionY, creature.PositionZ));
                        }
                    }
                    else if (questObjective.TargetGameObjectId != 0)
                    {
                        gameObjects = SqliteRepository.GetGameObjectsById(questObjective.TargetGameObjectId);
                    }
                    break;
                case 2:
                    if (questTemplate.ReqItemId2 != 0)
                    {
                        creatures = SqliteRepository.GetCreaturesByLootableItemId(questTemplate.ReqItemId2);
                        gameObjects = SqliteRepository.GetGameObjectByLootableItemId(questTemplate.ReqItemId2);

                        questObjective.ReqItemQty = questTemplate.ReqItemCount2;
                    }
                    else if (questObjective.TargetCreatureId != 0)
                    {
                        List<Creature> questCreatures = SqliteRepository.GetCreaturesById(questObjective.TargetCreatureId);

                        foreach (Creature creature in questCreatures)
                        {
                            questObjective.HotSpots.Add(new Position(creature.PositionX, creature.PositionY, creature.PositionZ));
                        }
                    }
                    else if (questObjective.TargetGameObjectId != 0)
                    {
                        gameObjects = SqliteRepository.GetGameObjectsById(questObjective.TargetGameObjectId);
                    }
                    break;
                case 3:
                    if (questTemplate.ReqItemId3 != 0)
                    {
                        creatures = SqliteRepository.GetCreaturesByLootableItemId(questTemplate.ReqItemId3);
                        gameObjects = SqliteRepository.GetGameObjectByLootableItemId(questTemplate.ReqItemId3);

                        questObjective.ReqItemQty = questTemplate.ReqItemCount3;
                    }
                    else if (questObjective.TargetCreatureId != 0)
                    {
                        List<Creature> questCreatures = SqliteRepository.GetCreaturesById(questObjective.TargetCreatureId);

                        foreach (Creature creature in questCreatures)
                        {
                            questObjective.HotSpots.Add(new Position(creature.PositionX, creature.PositionY, creature.PositionZ));
                        }
                    }
                    else if (questObjective.TargetGameObjectId != 0)
                    {
                        gameObjects = SqliteRepository.GetGameObjectsById(questObjective.TargetGameObjectId);
                    }
                    break;
                case 4:
                    if (questTemplate.ReqItemId4 != 0)
                    {
                        creatures = SqliteRepository.GetCreaturesByLootableItemId(questTemplate.ReqItemId4);
                        gameObjects = SqliteRepository.GetGameObjectByLootableItemId(questTemplate.ReqItemId4);

                        questObjective.ReqItemQty = questTemplate.ReqItemCount4;
                    }
                    else if (questObjective.TargetCreatureId != 0)
                    {
                        List<Creature> questCreatures = SqliteRepository.GetCreaturesById(questObjective.TargetCreatureId);

                        foreach (Creature creature in questCreatures)
                        {
                            questObjective.HotSpots.Add(new Position(creature.PositionX, creature.PositionY, creature.PositionZ));
                        }
                    }
                    else if (questObjective.TargetGameObjectId != 0)
                    {
                        gameObjects = SqliteRepository.GetGameObjectsById(questObjective.TargetGameObjectId);
                    }
                    break;
            }

            if (gameObjects.Count > 0 || creatures.Count > 0)
            {
                PopulateTargetHotSpots(questObjective, gameObjects, creatures);
            }

            return questObjective;
        }

        public static List<PlayerQuest> GetQuestsFromQuestLog()
        {
            List<PlayerQuest> questIds = new List<PlayerQuest>();

            for (int i = 0; i < 20; i++)
            {
                PlayerQuest playerQuest = ObjectManager.Player.GetPlayerQuestFromSlot(i);

                if (playerQuest.ID > 0)
                {
                    questIds.Add(playerQuest);
                }
            }

            return questIds;
        }

        private static void PopulateTargetHotSpots(QuestObjective questObjective, List<GameObject> gameObjects, List<Creature> creatures)
        {
            // Drops from mob
            if (creatures.Count > 0)
            {
                questObjective.TargetCreatureId = (ulong)creatures[0].Id;

                foreach (Creature creature in creatures)
                {
                    questObjective.HotSpots.Add(new Position(creature.PositionX, creature.PositionY, creature.PositionZ));
                }
            }
            // Looted from game object
            else if (gameObjects.Count > 0)
            {
                questObjective.TargetGameObjectId = (ulong)gameObjects[0].Id;

                foreach (GameObject gameObject in gameObjects)
                {
                    questObjective.HotSpots.Add(new Position(gameObject.PositionX, gameObject.PositionY, gameObject.PositionZ));
                }
            }
        }

        internal static bool IsObjectiveComplete(int questId, int index, int targetsNeeded)
        {
            PlayerQuest playerQuest = GetQuestsFromQuestLog().Where(x => x.ID == questId).First();

            switch (index)
            {
                case 1:
                    return playerQuest.ObjectiveRequiredCounts1 >= targetsNeeded;
                case 2:
                    return playerQuest.ObjectiveRequiredCounts2 >= targetsNeeded;
                case 3:
                    return playerQuest.ObjectiveRequiredCounts3 >= targetsNeeded;
                case 4:
                    return playerQuest.ObjectiveRequiredCounts4 >= targetsNeeded;
            }
            return false;
        }

        public readonly struct PlayerQuest
        {
            public readonly int ID;

            private readonly byte ObjectiveRequiredCountsSegment1;

            private readonly byte ObjectiveRequiredCountsSegment2;

            private readonly byte ObjectiveRequiredCountsSegment3;

            public byte ObjectiveRequiredCounts1
            {
                get
                {
                    byte total = 0;

                    if ((1 & ObjectiveRequiredCountsSegment1) == 1)
                    {
                        total++;
                    }
                    if ((2 & ObjectiveRequiredCountsSegment1) == 2)
                    {
                        total += 2;
                    }
                    if ((4 & ObjectiveRequiredCountsSegment1) == 4)
                    {
                        total += 4;
                    }
                    if ((8 & ObjectiveRequiredCountsSegment1) == 8)
                    {
                        total += 8;
                    }
                    if ((16 & ObjectiveRequiredCountsSegment1) == 16)
                    {
                        total += 16;
                    }
                    if ((32 & ObjectiveRequiredCountsSegment1) == 32)
                    {
                        total += 32;
                    }

                    return total;
                }
            }

            public byte ObjectiveRequiredCounts2
            {
                get
                {
                    byte total = 0;

                    if ((64 & ObjectiveRequiredCountsSegment1) == 64)
                    {
                        total++;
                    }
                    if ((128 & ObjectiveRequiredCountsSegment1) == 128)
                    {
                        total += 2;
                    }
                    if ((1 & ObjectiveRequiredCountsSegment2) == 1)
                    {
                        total += 4;
                    }
                    if ((2 & ObjectiveRequiredCountsSegment2) == 2)
                    {
                        total += 8;
                    }
                    if ((4 & ObjectiveRequiredCountsSegment2) == 4)
                    {
                        total += 16;
                    }
                    if ((8 & ObjectiveRequiredCountsSegment2) == 8)
                    {
                        total += 32;
                    }

                    return total;
                }
            }

            public byte ObjectiveRequiredCounts3
            {
                get
                {
                    byte total = 0;

                    if ((16 & ObjectiveRequiredCountsSegment2) == 16)
                    {
                        total++;
                    }
                    if ((32 & ObjectiveRequiredCountsSegment2) == 32)
                    {
                        total += 2;
                    }
                    if ((64 & ObjectiveRequiredCountsSegment2) == 64)
                    {
                        total += 4;
                    }
                    if ((128 & ObjectiveRequiredCountsSegment2) == 128)
                    {
                        total += 8;
                    }
                    if ((1 & ObjectiveRequiredCountsSegment3) == 1)
                    {
                        total += 16;
                    }
                    if ((2 & ObjectiveRequiredCountsSegment3) == 2)
                    {
                        total += 32;
                    }

                    return total;
                }
            }

            public byte ObjectiveRequiredCounts4
            {
                get
                {
                    byte total = 0;

                    if ((4 & ObjectiveRequiredCountsSegment3) == 4)
                    {
                        total++;
                    }
                    if ((8 & ObjectiveRequiredCountsSegment3) == 8)
                    {
                        total += 2;
                    }
                    if ((16 & ObjectiveRequiredCountsSegment3) == 16)
                    {
                        total += 4;
                    }
                    if ((32 & ObjectiveRequiredCountsSegment3) == 32)
                    {
                        total += 8;
                    }
                    if ((64 & ObjectiveRequiredCountsSegment3) == 64)
                    {
                        total += 16;
                    }
                    if ((128 & ObjectiveRequiredCountsSegment3) == 128)
                    {
                        total += 32;
                    }

                    return total;
                }
            }

            public readonly StateFlag State;

            public readonly int time;

            public enum StateFlag : byte
            {
                None,
                Complete,
                Failed
            }

            public override string ToString()
            {
                return ID + " " + +ObjectiveRequiredCounts1 + " " + ObjectiveRequiredCounts2 + " " + ObjectiveRequiredCounts3 + " " + ObjectiveRequiredCounts4 + " " + time + " " + State.ToString();
            }
        }
    }
}
