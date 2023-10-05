using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.AI.SharedStates
{
    public class StuckTask : BotTask, IBotTask
    {
        static readonly Stopwatch stopwatch = new Stopwatch();
        static readonly Random random = new Random();

        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;
        readonly Location startingLocation;

        State state = State.Stuck;

        public StuckTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.container = container;
            this.botTasks = botTasks;
            player = ObjectManager.Instance.Player;
            startingLocation = player.Location;
        }

        public void Update()
        {
            if (player.Location.GetDistanceTo(startingLocation) > 6)
            {
                StopMovement();
                botTasks.Pop();
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
