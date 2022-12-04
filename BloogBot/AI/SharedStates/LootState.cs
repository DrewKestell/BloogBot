using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Frames;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.AI.SharedStates
{
    public class LootState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;
        readonly int startTime = Environment.TickCount;

        int stuckCount;
        LootFrame lootFrame;
        int lootIndex;
        LootStates currentState;

        public LootState(
            Stack<IBotState> botStates,
            IDependencyContainer container,
            WoWUnit target)
        {
            this.botStates = botStates;
            this.container = container;
            this.target = target;
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(botStates, container);
        }

        public void Update()
        {
            if (player.Position.DistanceTo(target.Position) >= 5)
            {
                var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
                player.MoveToward(nextWaypoint);

                if (!player.IsImmobilized)
                {
                    if (stuckHelper.CheckIfStuck())
                        stuckCount++;
                }
            }

            if (target.CanBeLooted && currentState == LootStates.Initial && player.Position.DistanceTo(target.Position) < 5)
            {
                player.StopAllMovement();
                
                if (Wait.For("StartLootDelay", 200))
                {
                    target.Interact();
                    currentState = LootStates.RightClicked;
                    return;
                }
            }

            // State Transition Conditions:
            //  - target can't be looted (no items to loot)
            //  - loot frame is open, but we've already looted everything we want
            //  - stuck count is greater than 5 (perhaps the corpse is in an awkward position the character can't reach)
            //  - we've been in the loot state for over 10 seconds (again, perhaps the corpse is unreachable. most common example of this is when a mob dies on a cliff that we can't climb)
            if ((currentState == LootStates.Initial && !target.CanBeLooted) || (lootFrame != null && lootIndex == lootFrame.LootItems.Count) || stuckCount > 5 || Environment.TickCount - startTime > 10000)
            {
                player.StopAllMovement();
                botStates.Pop();
                botStates.Push(new EquipBagsState(botStates, container));
                if (player.IsSwimming)
                {
                    var nearestWaypoint = container
                        .Hotspots
                        .Where(h => h != null)
                        .SelectMany(h => h.Waypoints)
                        .OrderBy(w => player.Position.DistanceTo(w))
                        .FirstOrDefault();
                    if (nearestWaypoint != null)
                        botStates.Push(new MoveToPositionState(botStates, container, nearestWaypoint));
                }
                return;
            }

            if (currentState == LootStates.RightClicked && Wait.For("LootFrameDelay", 1000))
            {
                lootFrame = new LootFrame();
                currentState = LootStates.LootFrameReady;
            }

            if (currentState == LootStates.LootFrameReady && Wait.For("LootDelay", 150))
            {
                var itemToLoot = lootFrame.LootItems.ElementAt(lootIndex);
                var itemQuality = ItemQuality.Common;
                if (itemToLoot.Info != null)
                {
                    itemQuality = itemToLoot.Info.Quality;
                }

                var poorQualityCondition = itemToLoot.IsCoins || itemQuality == ItemQuality.Poor && container.BotSettings.LootPoor;
                var commonQualityCondition = itemToLoot.IsCoins || itemQuality == ItemQuality.Common && container.BotSettings.LootCommon;
                var uncommonQualityCondition = itemToLoot.IsCoins || itemQuality == ItemQuality.Uncommon && container.BotSettings.LootUncommon;
                var other = itemQuality != ItemQuality.Poor && itemQuality != ItemQuality.Common && itemQuality != ItemQuality.Uncommon;

                if (itemQuality == ItemQuality.Rare || itemQuality == ItemQuality.Epic)
                    DiscordClientWrapper.SendItemNotification(player.Name, itemQuality, itemToLoot.ItemId);

                if (itemToLoot.IsCoins
                    || ((string.IsNullOrWhiteSpace(container.BotSettings.LootExcludedNames) || !container.BotSettings.LootExcludedNames.Split('|').Any(en => itemToLoot.Info.Name.Contains(en)))
                    && (poorQualityCondition || commonQualityCondition || uncommonQualityCondition || other)))
                {
                    itemToLoot.Loot();
                }

                lootIndex++;
            }
        }
    }

    enum LootStates
    {
        Initial,
        RightClicked,
        LootFrameReady
    }
}
