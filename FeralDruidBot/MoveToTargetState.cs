using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace FeralDruidBot
{
    class MoveToTargetState : IBotState
    {
        const string Wrath = "Wrath";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        internal MoveToTargetState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target)
        {
            this.botStates = botStates;
            this.container = container;
            this.target = target;
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(botStates, container);
        }

        public void Update()
        {
            if (player.IsCasting)
            {
                return;
            }

            if (target.TappedByOther || container.FindClosestTarget()?.Guid != target.Guid)
            {
                player.StopAllMovement();
                Wait.RemoveAll();
                botStates.Pop();
                return;
            }

            stuckHelper.CheckIfStuck();

            if (player.Position.DistanceTo(target.Position) < 27 && player.InLosWith(target.Position))
            {
                if (player.IsMoving)
                    player.StopAllMovement();

                if (Wait.For("PullWithWrathDelay", 250))
                {
                    if (!player.IsInCombat)
                    {
                        player.LuaCall($"CastSpellByName('{Wrath}')");
                    }

                    Wait.RemoveAll();
                    botStates.Pop();
                    botStates.Push(new CombatState(botStates, container, target));
                }
                return;
            }

            var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
            player.MoveToward(nextWaypoint);
        }
    }
}
