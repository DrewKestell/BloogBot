using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

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
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(container, botTasks);
        }

        public void Update()
        {
            if (target.TappedByOther || (ObjectManager.Aggressors.Count() > 0 && !ObjectManager.Aggressors.Any(a => a.Guid == target.Guid)))
            {
                player.StopAllMovement();
                Wait.RemoveAll();
                botTasks.Pop();
                return;
            }

            stuckHelper.CheckIfStuck();
            
            if (player.Position.DistanceTo(target.Position) < 27 && !player.IsCasting && player.IsSpellReady(Wrath) && player.InLosWith(target.Position))
            {
                if (player.IsMoving)
                    player.StopAllMovement();

                if (Wait.For("PullWithWrathDelay", 100))
                {
                    if (!player.IsInCombat)
                        player.LuaCall($"CastSpellByName('{Wrath}')");

                    if (player.IsCasting || player.CurrentShapeshiftForm != "Human Form" || player.IsInCombat)
                    {
                        player.StopAllMovement();
                        Wait.RemoveAll();
                        botTasks.Pop();
                        botTasks.Push(new CombatTask(container, botTasks, new List<WoWUnit>() { target }));
                    }
                }
                return;
            }

            var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
            player.MoveToward(nextWaypoint);
        }
    }
}
