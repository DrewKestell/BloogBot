using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace ProtectionWarriorBot
{
    class MoveToTargetState : IBotState
    {
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
            if (target.TappedByOther || container.FindClosestTarget()?.Guid != target.Guid)
            {
                player.StopAllMovement();
                botStates.Pop();
                return;
            }

            stuckHelper.CheckIfStuck();

            var distanceToTarget = player.Position.DistanceTo(target.Position);
            if (distanceToTarget < 25 && distanceToTarget > 8 && !player.IsCasting && player.IsSpellReady("Charge") && player.InLosWith(target.Position))
            {
                if (!player.IsCasting)
                    player.LuaCall("CastSpellByName('Charge')");

                if (player.IsInCombat)
                {
                    player.StopAllMovement();
                    botStates.Pop();
                    botStates.Push(new CombatState(botStates, container, target));
                    return;
                }
            }

            if (distanceToTarget < 3)
            {
                player.StopAllMovement();
                botStates.Pop();
                botStates.Push(new CombatState(botStates, container, target));
                return;
            }

            var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
            player.MoveToward(nextWaypoint);
        }
    }
}
