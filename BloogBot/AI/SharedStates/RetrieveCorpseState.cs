using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.AI.SharedStates
{
    public class RetrieveCorpseState : IBotState
    {
        const int resDistance = 30;

        // res distance is around 36 units, so we build up a grid of 38 units 
        // in every direction, adding 1 to account for the center.
        static readonly int length = Convert.ToInt32(Math.Pow((resDistance * 2) + 1, 2.0));
        readonly Position[] resLocs = new Position[length];
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;

        bool initialized;

        public RetrieveCorpseState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if (!initialized)
            {
                // corpse position is wrong immediately after releasing, so we wait for 5s.
                //Thread.Sleep(5000);

                var resLocation = player.CorpsePosition;

                var threats = ObjectManager
                    .Units
                    .Where(u => u.Health > 0)
                    .Where(u => !u.TappedByOther)
                    .Where(u => !u.IsPet)
                    .Where(u => u.UnitReaction == UnitReaction.Hated || u.UnitReaction == UnitReaction.Hostile)
                    .Where(u => u.Level > player.Level - 10);

                if (threats.FirstOrDefault() != null)
                {
                    var index = 0;
                    var currentFloatX = player.CorpsePosition.X;
                    var currentFloatY = player.CorpsePosition.Y;
                    var currentFloatZ = player.CorpsePosition.Z;

                    for (var i = -resDistance; i <= resDistance; i++)
                    {
                        for (var j = -resDistance; j <= resDistance; j++)
                        {
                            resLocs[index] = new Position(currentFloatX + i, currentFloatY + j, currentFloatZ);
                            index++;
                        }
                    }

                    var maxDistance = 0f;

                    foreach (var resLoc in resLocs)
                    {
                        var path = Navigation.CalculatePath(ObjectManager.MapId, player.CorpsePosition, resLoc, false);
                        if (path.Length == 0) continue;
                        var endPoint = path[path.Length - 1];
                        var distanceToClosestThreat = endPoint.DistanceTo(threats.OrderBy(u => u.Position.DistanceTo(resLoc)).First().Position);

                        if (endPoint.DistanceTo(player.Position) < resDistance && distanceToClosestThreat > maxDistance)
                        {
                            maxDistance = distanceToClosestThreat;
                            resLocation = resLoc;
                        }
                    }
                }

                initialized = true;

                botStates.Push(new MoveToPositionState(botStates, container, resLocation, true));
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
                        botStates.Pop();
                        return;
                    }
                }
            }
        }
    }
}
