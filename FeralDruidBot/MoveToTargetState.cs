using BloogBot;
using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace FeralDruidBot
{
    class MoveToTargetState : MoveToTargetStateBase, IBotState
    {
        const string Wrath = "Wrath";
        const string FeralCharge = "Feral Charge - Cat";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        internal MoveToTargetState(
            Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) :
            base(botStates, container, target)
        {
            this.botStates = botStates;
            this.container = container;
            this.target = target;
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(botStates, container);
        }

        public new void Update()
        {
            if (player.IsCasting)
            {
                return;
            }

            if (base.Update())
            {
                Wait.RemoveAll();
                return;
            }

            stuckHelper.CheckIfStuck();

            if (player.Position.DistanceTo(target.Position) < 25 && player.InLosWith(target.Position))
            {
                if (player.IsMoving)
                    player.StopAllMovement();

                if (Wait.For("PullWithWrathDelay", 250))
                {
                    if (!player.IsInCombat && player.Level <= 12)
                    {
                        // Human form
                        player.LuaCall($"CastSpellByName('{Wrath}')");
                    }
                    else if (player.Level >= 20)
                    {
                        // Cat form
                        player.LuaCall($"CastSpellByName('{FeralCharge}')");
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
