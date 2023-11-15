using RaidMemberBot.Client;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.AI.SharedStates
{
    public class RetrieveCorpseTask : BotTask, IBotTask
    {
        const int resDistance = 30;

        // res distance is around 36 units, so we build up a grid of 38 units 
        // in every direction, adding 1 to account for the center.
        static readonly int length = Convert.ToInt32(Math.Pow((resDistance * 2) + 1, 2.0));
        readonly Position[] resLocs = new Position[length];
        readonly LocalPlayer player;

        bool initialized;

        public RetrieveCorpseTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary) { }

        public void Update()
        {
            if (ObjectManager.Player.Health > 0)
            {
                BotTasks.Pop();
            }

            if (!initialized)
            {
                // corpse position is wrong immediately after releasing, so we wait for 5s.
                //Thread.Sleep(5000);

                Position resPosition = ObjectManager.Player.CorpsePosition;

                IEnumerable<WoWUnit> threats = ObjectManager
                    .Units
                    .Where(u => u.Health > 0)
                    .Where(u => !u.TappedByOther)
                    .Where(u => !u.IsPet)
                    .Where(u => u.UnitReaction == UnitReaction.Unfriendly || u.UnitReaction == UnitReaction.Hostile)
                    .Where(u => u.Level > ObjectManager.Player.Level - 10);

                if (threats.FirstOrDefault() != null)
                {
                    int index = 0;
                    float currentFloatX = ObjectManager.Player.CorpsePosition.X;
                    float currentFloatY = ObjectManager.Player.CorpsePosition.Y;
                    float currentFloatZ = ObjectManager.Player.CorpsePosition.Z;

                    for (int i = -resDistance; i <= resDistance; i++)
                    {
                        for (int j = -resDistance; j <= resDistance; j++)
                        {
                            resLocs[index] = new Position(currentFloatX + i, currentFloatY + j, currentFloatZ);
                            index++;
                        }
                    }

                    float maxDistance = 0f;

                    foreach (Position resLoc in resLocs)
                    {
                        Position[] path = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.CorpsePosition, resLoc, false);
                        if (path.Length == 0) continue;
                        Position endPoint = path[path.Length - 1];
                        float distanceToClosestThreat = endPoint.DistanceTo(threats.OrderBy(u => u.Position.DistanceTo(resLoc)).First().Position);

                        if (endPoint.DistanceTo(ObjectManager.Player.Position) < resDistance && distanceToClosestThreat > maxDistance)
                        {
                            maxDistance = distanceToClosestThreat;
                            resPosition = resLoc;
                        }
                    }
                }

                initialized = true;

                Container.CurrentWaypoint = resPosition;
                BotTasks.Push(new MoveToWaypointTask(Container, BotTasks));
                return;
            }

            if (Wait.For("StartRetrieveCorpseStateDelay", 1000))
            {
                if (ObjectManager.Player.InGhostForm)
                    ObjectManager.Player.RetrieveCorpse();
                else
                {
                    if (Wait.For("LeaveRetrieveCorpseStateDelay", 2000))
                    {
                        BotTasks.Pop();
                        return;
                    }
                }
            }
        }
    }
}
