using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace FrostMageBot
{
    class MoveToTargetState : IBotState
    {
        const string waitKey = "FrostMagePull";

        const string Fireball = "Fireball";
        const string Frostbolt = "Frostbolt";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        readonly string pullingSpell;
        readonly int range;

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

            range = 28 + (ObjectManager.GetTalentRank(3, 11) * 3);
        }

        public void Update()
        {
            if (player.IsCasting)
                return;

            if (target.TappedByOther || container.FindClosestTarget()?.Guid != target.Guid)
            {
                player.StopAllMovement();
                botStates.Pop();
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
