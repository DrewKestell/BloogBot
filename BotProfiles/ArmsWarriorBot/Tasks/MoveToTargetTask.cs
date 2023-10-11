using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace ArmsWarriorBot
{
    class MoveToTargetTask : IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.botTasks = botTasks;
            this.container = container;
            this.target = target;
            player = ObjectManager.Instance.Player;
            stuckHelper = new StuckHelper(container, botTasks);
        }

        public void Update()
        {
            if (target.TappedByOther)
            {
                player.StopAllMovement();
                botTasks.Pop();
                return;
            }

            if (player.IsInCombat)
            {
                player.StopAllMovement();
                botTasks.Pop();
                botTasks.Push(new PvERotationTask(container, botTasks));
                return;
            }

            stuckHelper.CheckIfStuck();

            var distanceToTarget = player.Location.GetDistanceTo(target.Location);
            if (distanceToTarget < 25 && distanceToTarget > 8 && player.IsCasting && Spellbook.Instance.IsSpellReady("Charge") && player.InLosWith(target.Location))
            {
                if (player.IsCasting)
                {
                        Lua.Instance.Execute($"CastSpellByName('Charge')");
                }
            }

            if (distanceToTarget < 3)
            {
                player.StopAllMovement();
                botTasks.Pop();
                botTasks.Push(new PvERotationTask(container, botTasks));
                return;
            }

            var nextWaypoint = SocketClient.Instance.CalculatePath(player.MapId, player.Location, target.Location, false);
            player.MoveToward(nextWaypoint[0]);
        }
    }
}
