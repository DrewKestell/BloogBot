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

        State state = State.Initializing;

        public RetrieveCorpseState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if (state == State.Initializing)
            {
                if (!player.InGhostForm)
                {
                    // We are already alive. No need to do anything.
                    botStates.Pop();
                    return;
                }

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

                        if (endPoint.DistanceTo(player.CorpsePosition) < resDistance &&
                            distanceToClosestThreat > maxDistance &&
                            !attemptedResLocIndices.Contains(i))
                        {
                            maxDistance = distanceToClosestThreat;
                            maxDistanceIndex = i;
                            resLocation = resLocs[i];
                        }
                    }

                    attemptedResLocIndices.Add(maxDistanceIndex);
                }

                botStates.Push(new MoveToPositionState(
                    botStates, container, resLocation, true,
                    // Give it a minute to walk there. If we can't walk 30 yards in a minute, we're
                    // probably stuck.
                    deadline: Environment.TickCount + 60 * 1000));

                state = State.MovedToResLocation;
            }
            else if (state == State.MovedToResLocation)
            {
                // If here we are still too far from the corpse, it means we failed to move to the
                // res location. Let's try another one.
                if (player.Position.DistanceTo(player.CorpsePosition) > resDistance)
                {
                    state = State.Initializing;
                }
                else
                {
                    state = State.Resurrecting;
                }
            }
            else if (state == State.Resurrecting)
            {
                // Now we can res.
                if (Wait.For("StartRetrieveCorpseStateDelay", 1000))
                {
                    ObjectManager.Player.RetrieveCorpse();
                    state = State.ResClicked;
                }
            }
            else if (state == State.ResClicked)
            {
                if (Wait.For("LeaveRetrieveCorpseStateDelay", 2000))
                {
                    // In some cases we failed to res. E.g. when we checked for res distance we
                    // happened to be in range while falling off a cliff. In that case, we should
                    // try again.
                    if (player.InGhostForm)
                    {
                        state = State.Initializing;
                    }
                    else
                    {
                        botStates.Pop();
                    }
                }
            }
        }

        enum State
        {
            Initializing,
            MovedToResLocation,
            Resurrecting,
            ResClicked,
        }
    }
}
