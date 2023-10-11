using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace FeralDruidBot
{
    class MoveToTargetTask : IBotTask
    {
        const string Wrath = "Wrath";

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
            if (target.TappedByOther || (ObjectManager.Instance.Aggressors.Count() > 0 && !ObjectManager.Instance.Aggressors.Any(a => a.Guid == target.Guid)))
            {
                player.StopAllMovement();
                Wait.RemoveAll();
                botTasks.Pop();
                return;
            }

            stuckHelper.CheckIfStuck();
            
            if (player.Location.GetDistanceTo(target.Location) < 27 && player.IsCasting && Spellbook.Instance.IsSpellReady(Wrath) && player.InLosWith(target.Location))
            {
                if (player.IsMoving)
                    player.StopAllMovement();

                if (Wait.For("PullWithWrathDelay", 100))
                {
                    if (!player.IsInCombat)
                        Lua.Instance.Execute($"CastSpellByName('{Wrath}')");

                    if (player.IsCasting || player.CurrentShapeshiftForm != "Human Form" || player.IsInCombat)
                    {
                        player.StopAllMovement();
                        Wait.RemoveAll();
                        botTasks.Pop();
                        botTasks.Push(new PvERotationTask(container, botTasks));
                    }
                }
                return;
            }

            var nextWaypoint = SocketClient.Instance.CalculatePath(player.MapId, player.Location, target.Location, false);
            player.MoveToward(nextWaypoint[0]);
        }
    }
}
