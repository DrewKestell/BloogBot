using RaidMemberBot.Client;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Frames;
using RaidMemberBot.Game.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.AI.SharedStates
{
    public class LootTask : BotTask, IBotTask
    {
        readonly int startTime = Environment.TickCount;

        int stuckCount;
        LootFrame lootFrame;
        int lootIndex;
        LootStates currentState;

        public LootTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary) { }

        public void Update()
        {
            if (ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) >= 5)
            {
                Objects.Position[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);
                ObjectManager.Player.MoveToward(nextWaypoint[0]);
            }

            if (ObjectManager.Player.Target.CanBeLooted && currentState == LootStates.Initial && ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) < 5)
            {
                ObjectManager.Player.StopAllMovement();
                
                if (Wait.For("StartLootDelay", 200))
                {
                    ObjectManager.Player.Target.Interact();
                    currentState = LootStates.RightClicked;
                    return;
                }
            }

            // State Transition Conditions:
            //  - target can't be looted (no items to loot)
            //  - loot frame is open, but we've already looted everything we want
            //  - stuck count is greater than 5 (perhaps the corpse is in an awkward position the character can't reach)
            //  - we've been in the loot state for over 10 seconds (again, perhaps the corpse is unreachable. most common example of this is when a mob dies on a cliff that we can't climb)
            if ((currentState == LootStates.Initial && !ObjectManager.Player.Target.CanBeLooted) || (lootFrame != null && lootIndex == lootFrame.LootItems.Count) || stuckCount > 5 || Environment.TickCount - startTime > 10000)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new EquipBagsTask(Container, BotTasks));
                if (ObjectManager.Player.IsSwimming)
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
                LootItem itemToLoot = lootFrame.LootItems.ElementAt(lootIndex);
                ItemQuality itemQuality = (ItemQuality) itemToLoot.Info.Quality;

                bool poorQualityCondition = itemToLoot.IsCoins || itemQuality == ItemQuality.Poor;
                bool commonQualityCondition = itemToLoot.IsCoins || itemQuality == ItemQuality.Common;
                bool uncommonQualityCondition = itemToLoot.IsCoins || itemQuality == ItemQuality.Uncommon;
                bool other = itemQuality != ItemQuality.Poor && itemQuality != ItemQuality.Common && itemQuality != ItemQuality.Uncommon;

                //if (itemQuality == ItemQuality.Rare || itemQuality == ItemQuality.Epic)
                //    DiscordClientWrapper.SendItemNotification(ObjectManager.Player.Name, itemQuality, itemToLoot.ItemId);

                if (itemToLoot.IsCoins || (poorQualityCondition || commonQualityCondition || uncommonQualityCondition || other))
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
