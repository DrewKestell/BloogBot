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
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;

        bool initialized;

        public RetrieveCorpseTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.container = container;
            this.botTasks = botTasks;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if (ObjectManager.Instance.Player.Health > 0)
            {
                botTasks.Pop();
            }

            if (!initialized)
            {
                // corpse position is wrong immediately after releasing, so we wait for 5s.
                //Thread.Sleep(5000);

                var resLocation = player.CorpseLocation;

                var threats = ObjectManager
                    .Instance
                    .Units
                    .Where(u => u.Health > 0)
                    .Where(u => !u.TappedByOther)
                    .Where(u => !u.IsPet)
                    .Where(u => u.Reaction == UnitReaction.Hostile2 || u.Reaction == UnitReaction.Hostile)
                    .Where(u => u.Level > player.Level - 10);

                if (threats.FirstOrDefault() != null)
                {
                    var index = 0;
                    var currentFloatX = player.CorpseLocation.X;
                    var currentFloatY = player.CorpseLocation.Y;
                    var currentFloatZ = player.CorpseLocation.Z;

                    for (var i = -resDistance; i <= resDistance; i++)
                    {
                        for (var j = -resDistance; j <= resDistance; j++)
                        {
                            resLocs[index] = new Location(currentFloatX + i, currentFloatY + j, currentFloatZ);
                            index++;
                        }
                    }

                    var maxDistance = 0f;

                    foreach (var resLoc in resLocs)
                    {
                        var path = SocketClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, player.CorpseLocation, resLoc, false);
                        if (path.Length == 0) continue;
                        var endPoint = path[path.Length - 1];
                        var distanceToClosestThreat = endPoint.GetDistanceTo(threats.OrderBy(u => u.Location.GetDistanceTo(resLoc)).First().Location);

                        if (endPoint.GetDistanceTo(player.Location) < resDistance && distanceToClosestThreat > maxDistance)
                        {
                            maxDistance = distanceToClosestThreat;
                            resLocation = resLoc;
                        }
                    }
                }

                initialized = true;

                botTasks.Push(new MoveToLocationTask(container, botTasks, resLocation));
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
                        botTasks.Pop();
                        return;
                    }
                }
            }
        }
    }
}
