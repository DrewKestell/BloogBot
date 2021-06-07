using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace ShadowPriestBot
{
    class MoveToTargetState : IBotState
    {
        const string HolyFire = "Holy Fire";
        const string MindBlast = "Mind Blast";
        const string PowerWordShield = "Power Word: Shield";
        const string ShadowForm = "Shadowform";
        const string Smite = "Smite";
        const string WeakenedSoul = "WeakenedSoul";

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

            if (player.HasBuff(ShadowForm))
                pullingSpell = MindBlast;
            else if (player.KnowsSpell(HolyFire))
                pullingSpell = HolyFire;
            else
                pullingSpell = Smite;
        }

        public void Update()
        {
            if (target.TappedByOther || container.FindClosestTarget()?.Guid != target.Guid)
            {
                player.StopAllMovement();
                botStates.Pop();
                return;
            }

            var distanceToTarget = player.Position.DistanceTo(target.Position);
            if (distanceToTarget < 27)
            {
                if (player.IsMoving)
                    player.StopAllMovement();

                if (!player.IsCasting && player.IsSpellReady(pullingSpell))
                {
                    if (!player.KnowsSpell(PowerWordShield) || player.HasBuff(PowerWordShield) || player.IsInCombat)
                    {
                        if (Wait.For("ShadowPriestPullDelay", 250))
                        {
                            player.SetTarget(target.Guid);
                            Wait.Remove("ShadowPriestPullDelay");

                            if (!player.IsInCombat)
                                player.LuaCall($"CastSpellByName('{pullingSpell}')");

                            player.StopAllMovement();
                            botStates.Pop();
                            botStates.Push(new CombatState(botStates, container, target));
                        }
                    }

                    if (player.KnowsSpell(PowerWordShield) && !player.HasDebuff(WeakenedSoul) && !player.HasBuff(PowerWordShield))
                        player.LuaCall($"CastSpellByName('{PowerWordShield}',1)");

                    return;
                }
            }
            else
            {
                stuckHelper.CheckIfStuck();

                var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
                player.MoveToward(nextWaypoint);
            }
        }
    }
}
