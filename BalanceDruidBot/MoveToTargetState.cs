using BloogBot;
using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BalanceDruidBot
{
    class MoveToTargetState : MoveToTargetStateBase, IBotState
    {
        const string Wrath = "Wrath";
        const string Starfire = "Starfire";
        const string MoonkinForm = "Moonkin Form";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        readonly int range;
        readonly string pullingSpell;

        internal MoveToTargetState(
            Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) :
            base(botStates, container, target)
        {
            this.botStates = botStates;
            this.container = container;
            this.target = target;
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(botStates, container);

            if (player.Level <= 19)
                range = 28;
            else if (player.Level == 20)
                range = 31;
            else
                range = 34;

            if (player.KnowsSpell(Starfire))
                pullingSpell = Starfire;
            else
                pullingSpell = Wrath;
        }

        public new void Update()
        {
            if (base.Update())
            {
                Wait.RemoveAll();
                return;
            }

            stuckHelper.CheckIfStuck();

            if (player.IsCasting)
                return;

            if (player.KnowsSpell(MoonkinForm) && !player.HasBuff(MoonkinForm))
            {
                player.LuaCall($"CastSpellByName('{MoonkinForm}')");
            }

            if (player.Position.DistanceTo(target.Position) < range && !player.IsCasting && player.IsSpellReady(pullingSpell) && player.InLosWith(target.Position))
            {
                if (player.IsMoving)
                    player.StopAllMovement();

                if (Wait.For("BalanceDruidPullDelay", 100))
                {
                    if (!player.IsInCombat)
                        player.LuaCall($"CastSpellByName('{pullingSpell}')");

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
