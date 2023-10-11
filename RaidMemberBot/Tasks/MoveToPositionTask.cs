using Newtonsoft.Json;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.AI.SharedStates
{
    public class MoveToLocationTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly Location destination;
        readonly bool use2DPop;
        readonly LocalPlayer player;
        readonly int startingMapId;

        int stuckCount;
        Location currentWaypoint;

        public MoveToLocationTask(IClassContainer container, Stack<IBotTask> botTasks, Location destination, bool use2DPop = false)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.destination = destination;
            this.use2DPop = use2DPop;

            player = ObjectManager.Instance.Player;
            currentWaypoint = ObjectManager.Instance.Player.Location;
            startingMapId = (int)ObjectManager.Instance.Player.MapId;
        }

        public void Update()
        {
            if (player.IsInCombat)
            {
                player.StopAllMovement();
                botTasks.Push(container.CreatePvERotationTask(container, botTasks));
                return;
            }

            if (use2DPop)
            {
                if (player.Location.GetDistanceTo2D(destination) < 3 || stuckCount > 20)
                {
                    player.StopAllMovement();
                    botTasks.Pop();
                    return;
                }
            }
            else
            {
                if (player.Location.GetDistanceTo(destination) < 1 || stuckCount > 20 || startingMapId != ObjectManager.Instance.Player.MapId)
                {
                    player.StopAllMovement();
                    botTasks.Pop();
                    return;
                }
            }

            var nextWaypoint = SocketClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, player.Location, destination, false);
            if (nextWaypoint.Length > 1)
            {
                currentWaypoint = nextWaypoint[1];
            }
            else
            {
                botTasks.Pop();
                return;
            }

            player.MoveToward(currentWaypoint);
        }
    }
}
