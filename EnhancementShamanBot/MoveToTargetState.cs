using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace EnhancementShamanBot
{
    class MoveToTargetState : IBotState
    {
        const string LightningBolt = "Lightning Bolt";

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
            if (target.TappedByOther || (ObjectManager.Aggressors.Count() > 0 && !ObjectManager.Aggressors.Any(a => a.Guid == target.Guid)))
            {
                Wait.RemoveAll();
                botStates.Pop();
                return;
            }

            stuckHelper.CheckIfStuck();

            if (player.Position.DistanceTo(target.Position) < 27 && !player.IsCasting && player.IsSpellReady(LightningBolt) && player.InLosWith(target.Position))
            {
                if (player.IsMoving)
                    player.StopAllMovement();

                if (Wait.For("PullWithLightningBoltDelay", 100))
                {
                    if (!player.IsInCombat)
                        player.LuaCall($"CastSpellByName('{LightningBolt}')");

                    if (player.IsCasting || player.IsInCombat)
                    {
                        player.StopAllMovement();
                        Wait.RemoveAll();
                        botStates.Pop();
                        botStates.Push(new CombatState(botStates, container, target));
                    }
                }
                return;
            }

            var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
            player.MoveToward(nextWaypoint);
        }
    }
}
