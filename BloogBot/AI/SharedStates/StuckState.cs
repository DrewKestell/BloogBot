using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BloogBot.AI.SharedStates
{
    public class StuckState : IBotState
    {
        static readonly Stopwatch stopwatch = new Stopwatch();
        static readonly Random random = new Random();

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;
        readonly Position startingPosition;

        State state = State.Stuck;

        public StuckState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
            startingPosition = player.Position;
        }

        public void Update()
        {
            if (player.Position.DistanceTo(startingPosition) > 3)
            {
                StopMovement();
                botStates.Pop();
                return;
            }

            if (state == State.Moving)
            {
                if (stopwatch.ElapsedMilliseconds > 150)
                    state = State.Stuck;
                return;
            }
                
            var ran = random.Next(0, 4);
            state = State.Moving;
            stopwatch.Restart();
            StopMovement();

            if (ran == 0)
            {
                player.StartMovement(ControlBits.Front);
                player.StartMovement(ControlBits.StrafeLeft);
                player.Jump();
            }
            if (ran == 1)
            {
                player.StartMovement(ControlBits.Front);
                player.StartMovement(ControlBits.StrafeRight);
                player.Jump();
            }
            if (ran == 2)
            {
                player.StartMovement(ControlBits.Back);
                player.StartMovement(ControlBits.StrafeLeft);
                player.Jump();
            }
            if (ran == 3)
            {
                player.StartMovement(ControlBits.Back);
                player.StartMovement(ControlBits.StrafeRight);
                player.Jump();
            }
        }

        void StopMovement()
        {
            player.StopMovement(ControlBits.Front);
            player.StopMovement(ControlBits.Back);
            player.StopMovement(ControlBits.StrafeLeft);
            player.StopMovement(ControlBits.StrafeRight);
        }

        enum State
        {
            Stuck,
            Moving
        }
    }
}
