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
        readonly HashSet<int> attemptedResLocIndices = new HashSet<int>();
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;

        bool initialized;
        bool resurrecting = false;

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
                    int maxDistanceIndex = -1;

                    for (int i = 0; i < resLocs.Length; i++)
                    {
                        var path = Navigation.CalculatePath(ObjectManager.MapId, player.Position, resLocs[i], false);
                        if (path.Length == 0) continue;
                        var endPoint = path[path.Length - 1];
                        var distanceToClosestThreat = endPoint.DistanceTo(threats.OrderBy(u => u.Position.DistanceTo(resLocs[i])).First().Position);

                        if (endPoint.DistanceTo(player.CorpsePosition) < resDistance && distanceToClosestThreat > maxDistance && !attemptedResLocIndices.Contains(i))
                        {
                            maxDistance = distanceToClosestThreat;
                            maxDistanceIndex = i;
                            resLocation = resLocs[i];
                        }
                    }

                    attemptedResLocIndices.Add(maxDistanceIndex);
                }

                initialized = true;

                botStates.Push(new MoveToPositionState(
                    botStates, container, resLocation, true,
                    // Give it a minute to walk there. If we can't walk 30 yards in a minute, we're
                    // probably stuck.
                    deadline: Environment.TickCount + 60 * 1000));

                return;
            }

            // If here we are still too far from the corpse, it means we failed to move to the res
            // location. Let's try another one.
            if (!resurrecting && player.Position.DistanceTo(player.CorpsePosition) > resDistance)
            {
                initialized = false;
                return;
            }
            else
            {
                // We must stop checking res distance once we confirmed we are at where we want
                // because after res the corpse position will become (0, 0). We don't want to set
                // initialized to false in next update.
                resurrecting = true;
            }

            // Now we can res.
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
