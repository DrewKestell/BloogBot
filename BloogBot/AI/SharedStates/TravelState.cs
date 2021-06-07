using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;

namespace BloogBot.AI.SharedStates
{
    public class TravelState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly Position[] travelPathWaypoints;
        readonly Action callback;
        readonly LocalPlayer player;
        
        int travelPathIndex;

        public TravelState(Stack<IBotState> botStates, IDependencyContainer container, Position[] travelPathWaypoints, int startingIndex, Action callback = null)
        {
            this.botStates = botStates;
            this.container = container;
            this.travelPathWaypoints = travelPathWaypoints;
            this.callback = callback;
            player = ObjectManager.Player;
            travelPathIndex = startingIndex;
        }

        public void Update()
        {
            var threat = container.FindThreat();

            if (threat != null)
            {
                player.StopAllMovement();
                botStates.Push(container.CreateMoveToTargetState(botStates, container, threat));
                return;
            }
            
            if (player.Position.DistanceTo(travelPathWaypoints[travelPathIndex]) < 3)
                travelPathIndex++;

            if (travelPathIndex == travelPathWaypoints.Length)
            {
                player.StopAllMovement();
                botStates.Pop();
                callback?.Invoke();
                return;
            }
            
            player.MoveToward(travelPathWaypoints[travelPathIndex]);
        }
    }
}
