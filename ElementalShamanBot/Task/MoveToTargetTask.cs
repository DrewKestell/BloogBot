using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ElementalShamanBot
{
    class MoveToTargetTask : IBotTask
    {
        const string LightningBolt = "Lightning Bolt";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        int stuckCount;

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
                botTasks.Pop();
                return;
            }
            if (stuckHelper.CheckIfStuck())
                stuckCount++;

            if (stuckCount > 20)
            {
                player.StopAllMovement();
                botTasks.Pop();
                return;
            } 

            if (player.Position.DistanceTo(target.Position) < 30 && !player.IsCasting && player.IsSpellReady(LightningBolt) && player.InLosWith(target.Position))
            {
                player.StopAllMovement();
                botTasks.Pop();
                botTasks.Push(new CombatTask(container, botTasks, new List<WoWUnit>() { target }));
                return;
            }

            var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
            player.MoveToward(nextWaypoint);
        }
    }
}
