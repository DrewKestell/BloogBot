// Friday owns this file!

using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace BeastMasterHunterBot
{
    class MoveToTargetState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        const string GunLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Auto Shot') end";
        const string SerpentSting = "Serpent Sting";
        const string AspectOfTheMonkey = "Aspect Of The Monkey";
        const string AspectOfTheCheetah = "Aspect Of The Cheetah";




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
            if (distanceToTarget < 33 && !player.IsCasting)
            {
                player.StopAllMovement();
                player.LuaCall(GunLuaScript);
                botStates.Pop();
                botStates.Push(new CombatState(botStates, container, target));
                return;
            }

            var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
            player.MoveToward(nextWaypoint);
        }
    }
}
