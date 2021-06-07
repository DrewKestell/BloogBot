using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace ArcaneMageBot
{
    class MoveToTargetState : IBotState
    {
        const string Fireball = "Fireball";
        const string Frostbolt = "Frostbolt";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        readonly string pullingSpell;

        internal MoveToTargetState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target)
        {
            this.botStates = botStates;
            this.container = container;
            this.target = target;
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(botStates, container);

            if (player.KnowsSpell(Frostbolt))
                pullingSpell = Frostbolt;
            else
                pullingSpell = Fireball;
        }

        public void Update()
        {
            if (target.TappedByOther || container.FindClosestTarget()?.Guid != target.Guid)
            {
                player.StopAllMovement();
                botStates.Pop();
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
                    botStates.Pop();
                    botStates.Push(new CombatState(botStates, container, target));
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
