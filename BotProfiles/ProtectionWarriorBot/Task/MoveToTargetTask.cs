using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace ProtectionWarriorBot
{
    class MoveToTargetTask : IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;
        WoWUnit target;

        Location currentWaypoint;
        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.botTasks = botTasks;
            this.container = container;
            this.target = target;
            player = ObjectManager.Instance.Player;
            currentWaypoint = ObjectManager.Instance.Player.Location;
        }

        public void Update()
        {
            if (ObjectManager.Instance.Hostiles.Where(x => player.InLosWith(x)).ToList().Count > 0)
            {
                WoWUnit potentialNewTarget = ObjectManager.Instance.Hostiles.Where(x => player.InLosWith(x)).First();

                if (potentialNewTarget != null && potentialNewTarget.Guid != target.Guid && player.InLosWith(potentialNewTarget))
                {
                    target = potentialNewTarget;
                    player.SetTarget(potentialNewTarget);
                }
            }

            if (player.CurrentStance != "Battle Stance")
                Lua.Instance.Execute("CastSpellByName('Battle Stance')");

            var distanceToTarget = player.Location.GetDistanceTo(target.Location);

            if (distanceToTarget < 25 && distanceToTarget > 8 && !player.IsCasting && Spellbook.Instance.IsSpellReady("Charge") && player.InLosWith(target.Location))
            {
                if (!player.IsCasting)
                    Lua.Instance.Execute("CastSpellByName('Charge')");

                if (player.IsInCombat)
                {
                    player.StopAllMovement();
                    botTasks.Pop();
                    botTasks.Push(new PvERotationTask(container, botTasks));
                    return;
                }
            }

            if (distanceToTarget < 3)
            {
                player.StopAllMovement();
                botTasks.Pop();
                botTasks.Push(new PvERotationTask(container, botTasks));
                return;
            }

            var nextWaypoint = SocketClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, player.Location, target.Location, true);

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
