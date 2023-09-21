using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Diagnostics;

namespace BloogBot.AI.SharedStates
{
    public class UseItemOnUnitState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit targetUnit;
        readonly WoWItem usableItem;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        int stuckCount;

        public UseItemOnUnitState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target, WoWItem wowItem)
        {
            this.botStates = botStates;
            this.container = container;
            this.targetUnit = target;
            this.usableItem = wowItem;
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(botStates, container);
        }

        public void Update()
        {
            if (CheckForQuestEntitiesState.TargetGuidBlacklist.ContainsKey(targetUnit.Guid))
            {
                botStates.Pop();
                return;
            }

            var threat = container.FindThreat();

            if (threat != null)
            {
                player.StopAllMovement();
                botStates.Push(container.CreateMoveToTargetState(botStates, container, threat));
                return;
            }

            if (stuckHelper.CheckIfStuck())
                stuckCount++;

            if (player.TargetGuid != targetUnit.Guid)
            {
                player.Target = targetUnit;
            }


            if (player.Position.DistanceTo(targetUnit.Position) < 3 || stuckCount > 20)
            {
                player.StopAllMovement();
                usableItem.Use();

                if (Wait.For("ItemUseAnim", 1000))
                {
                    CheckForQuestEntitiesState.TargetGuidBlacklist.Add(targetUnit.Guid, Stopwatch.StartNew());
                    botStates.Pop();
                    return;
                }
            }
            else
            {
                var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, targetUnit.Position, false);
                player.MoveToward(nextWaypoint);
            }

        }
    }
}
