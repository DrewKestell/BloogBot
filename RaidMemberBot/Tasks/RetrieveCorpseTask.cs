using RaidMemberBot.Client;
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
        readonly Location[] resLocs = new Location[length];
        readonly LocalPlayer player;

        bool initialized;

        public RetrieveCorpseTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary) { }

        public void Update()
        {
            if (ObjectManager.Instance.Player.Health > 0)
            {
                BotTasks.Pop();
            }

            if (!initialized)
            {
                // corpse position is wrong immediately after releasing, so we wait for 5s.
                //Thread.Sleep(5000);

                Location resLocation = Container.Player.CorpseLocation;

                IEnumerable<WoWUnit> threats = ObjectManager
                    .Instance
                    .Units
                    .Where(u => u.Health > 0)
                    .Where(u => !u.TappedByOther)
                    .Where(u => !u.IsPet)
                    .Where(u => u.Reaction == UnitReaction.Hostile2 || u.Reaction == UnitReaction.Hostile)
                    .Where(u => u.Level > Container.Player.Level - 10);

                if (threats.FirstOrDefault() != null)
                {
                    int index = 0;
                    float currentFloatX = Container.Player.CorpseLocation.X;
                    float currentFloatY = Container.Player.CorpseLocation.Y;
                    float currentFloatZ = Container.Player.CorpseLocation.Z;

                    for (int i = -resDistance; i <= resDistance; i++)
                    {
                        for (int j = -resDistance; j <= resDistance; j++)
                        {
                            resLocs[index] = new Location(currentFloatX + i, currentFloatY + j, currentFloatZ);
                            index++;
                        }
                    }

                    float maxDistance = 0f;

                    foreach (Location resLoc in resLocs)
                    {
                        Location[] path = NavigationClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, Container.Player.CorpseLocation, resLoc, false);
                        if (path.Length == 0) continue;
                        Location endPoint = path[path.Length - 1];
                        float distanceToClosestThreat = endPoint.GetDistanceTo(threats.OrderBy(u => u.Location.GetDistanceTo(resLoc)).First().Location);

                        if (endPoint.GetDistanceTo(Container.Player.Location) < resDistance && distanceToClosestThreat > maxDistance)
                        {
                            maxDistance = distanceToClosestThreat;
                            resLocation = resLoc;
                        }
                    }
                }

                initialized = true;

                Container.CurrentWaypoint = resLocation;
                BotTasks.Push(new MoveToWaypointTask(Container, BotTasks));
                return;
            }

            if (Wait.For("StartRetrieveCorpseStateDelay", 1000))
            {
                if (ObjectManager.Instance.Player.InGhostForm)
                    ObjectManager.Instance.Player.RetrieveCorpse();
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
