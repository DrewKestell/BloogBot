using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace CombatRogueBot
{
    class MoveToTargetState : IBotState
    {
        const string Distract = "Distract";
        const string Garrote = "Garrote";
        const string Stealth = "Stealth";
        const string CheapShot = "Cheap Shot";

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

            if (!player.IsImmobilized)
                stuckHelper.CheckIfStuck();

            var distanceToTarget = player.Position.DistanceTo(target.Position);
            if (distanceToTarget < 25 && !player.HasBuff(Stealth) && player.KnowsSpell(Garrote) && !player.IsInCombat)
            {
                player.LuaCall($"CastSpellByName('{Stealth}')");
            }

            if (distanceToTarget < 15 && player.KnowsSpell(Distract) && player.IsSpellReady(Distract) && target.CreatureType != CreatureType.Totem)
            {
                var delta = target.Position - player.Position;
                var normalizedVector = delta.GetNormalizedVector();
                var scaledVector = normalizedVector * 5;
                var targetPosition = target.Position + scaledVector;

                player.CastSpellAtPosition(Distract, targetPosition);
            }

            if (distanceToTarget < 3.5 && player.HasBuff(Stealth) && !player.IsInCombat && target.CreatureType != CreatureType.Totem)
            {
                if (player.IsSpellReady(Garrote) && target.CreatureType != CreatureType.Elemental && player.IsBehind(target))
                {
                    player.LuaCall($"CastSpellByName('{Garrote}')");
                    return;
                }
                else if (player.IsSpellReady(CheapShot) && player.IsBehind(target))
                {
                    player.LuaCall($"CastSpellByName('{CheapShot}')");
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
