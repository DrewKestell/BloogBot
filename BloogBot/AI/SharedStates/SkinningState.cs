using System;
using System.Collections.Generic;
using System.Linq;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Frames;
using BloogBot.Game.Objects;

namespace BloogBot.AI.SharedStates
{
    public class SkinningState : IBotState
    {
        const string Skinning = "Skinning";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly int startTime = Environment.TickCount;

        State state = State.Initial;
        LootFrame lootFrame;
        int lootIndex = 0;

        public SkinningState(
            Stack<IBotState> botStates,
            IDependencyContainer container,
            WoWUnit target)
        {
            this.botStates = botStates;
            this.container = container;
            this.target = target;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            // State Transition Conditions:
            //  - we don't know skinning
            //  - loot frame is open, but we've already looted everything we want
            //  - we've been in the loot state for over 10 seconds (again, perhaps the corpse is unreachable. most common example of this is when a mob dies on a cliff that we can't climb)
            if (!player.KnowsSpell(Skinning) ||
                (lootFrame != null && lootIndex >= lootFrame.LootItems.Count) ||
                Environment.TickCount - startTime > 10 * 1000)
            {
                Exit();
                return;
            }

            if (state == State.Initial)
            {
                if (Wait.For("StartSkinningDelay", 200))
                {
                    target.Interact();
                    state = State.StartingSkinning;
                }
            }
            else if (state == State.StartingSkinning)
            {
                if (Wait.For("WaitingForSkinningCastDelay", 500))
                {
                    if (!player.IsCasting)
                    {
                        // We probably failed to skin for some reason (e.g. insufficient skill level).
                        Exit();
                        return;
                    }
                    state = State.Skinning;
                }
            }
            else if (state == State.Skinning)
            {
                if (Wait.For("SkinningDelay", 2000))
                {
                    state = State.LootFrameReady;
                    lootFrame = new LootFrame();
                }
            }
            else if (state == State.LootFrameReady)
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

        void Exit()
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
        }

        enum State
        {
            Initial,
            StartingSkinning,
            Skinning,
            LootFrameReady,
        }
    }
}
