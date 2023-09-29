using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace FrostMageBot
{
    class MoveToTargetTask : IBotTask
    {
        const string waitKey = "FrostMagePull";

        const string Fireball = "Fireball";
        const string Frostbolt = "Frostbolt";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        readonly string pullingSpell;
        readonly int range;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.botTasks = botTasks;
            this.container = container;
            this.target = target;
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(container, botTasks);

            if (player.KnowsSpell(Frostbolt))
                pullingSpell = Frostbolt;
            else
                pullingSpell = Fireball;

            range = 28 + (ObjectManager.GetTalentRank(3, 11) * 3);
        }

        public void Update()
        {
            if (player.IsCasting)
                return;

            if (target.TappedByOther)
            {
                player.StopAllMovement();
                botTasks.Pop();
                return;
            }

            stuckHelper.CheckIfStuck();

            var distanceToTarget = player.Position.DistanceTo(target.Position);
            if (distanceToTarget <= range && player.InLosWith(target.Position))
            {
                if (player.IsMoving)
                    player.StopAllMovement();

                if (Wait.For(waitKey, 250))
                {
                    player.StopAllMovement();
                    Wait.Remove(waitKey);
                    
                    if (!player.IsInCombat)
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
