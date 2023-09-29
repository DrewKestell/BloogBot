using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace ArcaneMageBot
{
    class MoveToTargetTask : IBotTask
    {
        const string Fireball = "Fireball";
        const string Frostbolt = "Frostbolt";

        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        readonly string pullingSpell;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.target = target;
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(container, botTasks);

            if (player.KnowsSpell(Frostbolt))
                pullingSpell = Frostbolt;
            else
                pullingSpell = Fireball;
        }

        public void Update()
        {
            if (target.TappedByOther)
            {
                player.StopAllMovement();
                botTasks.Pop();
                return;
            }

            stuckHelper.CheckIfStuck();

            var distanceToTarget = player.Position.DistanceTo(target.Position);
            if (distanceToTarget < 27)
            {
                if (player.IsMoving)
                    player.StopAllMovement();

                if (!player.IsCasting && player.IsSpellReady(pullingSpell) && Wait.For("ArcaneMagePull", 500))
                {
                    player.StopAllMovement();
                    Wait.RemoveAll();
                    player.LuaCall($"CastSpellByName('{pullingSpell}')");
                    botTasks.Pop();
                    botTasks.Push(new CombatTask(container, botTasks, new List<WoWUnit>() { target }));
                    return;
                }
            }
            else
            {
                var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
                player.MoveToward(nextWaypoint);
            }
        }
    }
}
