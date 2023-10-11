using RaidMemberBot.Client;
using RaidMemberBot.Game.Frames;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.AI.SharedStates
{
    public class LootTask : BotTask, IBotTask
    {
        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;
        readonly int startTime = Environment.TickCount;

        int stuckCount;
        LootFrame lootFrame;
        int lootIndex;
        LootStates currentState;

        public LootTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.target = target;
            player = ObjectManager.Instance.Player;
            stuckHelper = new StuckHelper(container, botTasks);
        }

        public void Update()
        {
            if (player.Location.GetDistanceTo(target.Location) >= 5)
            {
                var nextWaypoint = SocketClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, player.Location, target.Location, false);
                player.MoveToward(nextWaypoint[0]);

                if (!player.IsInCC)
                {
                    if (stuckHelper.CheckIfStuck())
                        stuckCount++;
                }
            }

            if (target.CanBeLooted && currentState == LootStates.Initial && player.Location.GetDistanceTo(target.Location) < 5)
            {
                player.StopAllMovement();
                
                if (Wait.For("StartLootDelay", 200))
                {
                    target.Interact(true);
                    currentState = LootStates.RightClicked;
                    return;
                }
            }

            // State Transition Conditions:
            //  - target can't be looted (no items to loot)
            //  - loot frame is open, but we've already looted everything we want
            //  - stuck count is greater than 5 (perhaps the corpse is in an awkward position the character can't reach)
            //  - we've been in the loot state for over 10 seconds (again, perhaps the corpse is unreachable. most common example of this is when a mob dies on a cliff that we can't climb)
            if ((currentState == LootStates.Initial && !target.CanBeLooted) || (lootFrame != null && lootIndex == lootFrame.LootCount) || stuckCount > 5 || Environment.TickCount - startTime > 10000)
            {
                player.StopAllMovement();
                botTasks.Pop();
                botTasks.Push(new EquipBagsTask(container, botTasks));
                if (player.IsSwimming)
                {

                }
                return;
            }

            if (currentState == LootStates.RightClicked && Wait.For("LootFrameDelay", 1000))
            {
                
                currentState = LootStates.LootFrameReady;
            }

            if (currentState == LootStates.LootFrameReady && Wait.For("LootDelay", 150))
            {
                var itemToLoot = lootFrame.Items.ElementAt(lootIndex);
                var itemQuality = (ItemQuality) itemToLoot.Info.Quality;

                var poorQualityCondition = itemToLoot.IsCoin || itemQuality == ItemQuality.Poor;
                var commonQualityCondition = itemToLoot.IsCoin || itemQuality == ItemQuality.Common;
                var uncommonQualityCondition = itemToLoot.IsCoin || itemQuality == ItemQuality.Uncommon;
                var other = itemQuality != ItemQuality.Poor && itemQuality != ItemQuality.Common && itemQuality != ItemQuality.Uncommon;

                //if (itemQuality == ItemQuality.Rare || itemQuality == ItemQuality.Epic)
                //    DiscordClientWrapper.SendItemNotification(player.Name, itemQuality, itemToLoot.ItemId);

                if (itemToLoot.IsCoin || (poorQualityCondition || commonQualityCondition || uncommonQualityCondition || other))
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
