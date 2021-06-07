using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace AfflictionWarlockBot
{
    class MoveToTargetState : IBotState
    {
        const string SummonImp = "Summon Imp";
        const string SummonVoidwalker = "Summon Voidwalker";
        const string CurseOfAgony = "Curse of Agony";
        const string ShadowBolt = "Shadow Bolt";
        
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

            if (player.KnowsSpell(CurseOfAgony))
                pullingSpell = CurseOfAgony;
            else
                pullingSpell = ShadowBolt;
        }

        public void Update()
        {
            if (target.TappedByOther || container.FindClosestTarget()?.Guid != target.Guid)
            {
                player.StopAllMovement();
                botStates.Pop();
                return;
            }

            if (ObjectManager.Pet == null && (player.KnowsSpell(SummonImp) || player.KnowsSpell(SummonVoidwalker)))
            {
                player.StopAllMovement();
                botStates.Push(new SummonVoidwalkerState(botStates));
                return;
            }

            stuckHelper.CheckIfStuck();

            var distanceToTarget = player.Position.DistanceTo(target.Position);
            if (distanceToTarget < 27 && !player.IsCasting && player.IsSpellReady(pullingSpell))
            {
                if (player.IsMoving)
                    player.StopAllMovement();

                if (Wait.For("AfflictionWarlockPullDelay", 250))
                {
                    player.StopAllMovement();
                    player.LuaCall($"CastSpellByName('{pullingSpell}')");
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
