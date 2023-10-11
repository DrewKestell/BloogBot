// Nat owns this file!

using System.Collections.Generic;
using System;
using RaidMemberBot.Objects;
using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using static RaidMemberBot.Constants.Enums;
using RaidMemberBot;
using RaidMemberBot.Client;

namespace BackstabRogueBot
{
    class MoveToTargetTask : IBotTask
    {
        const string Distract = "Distract";
        const string Garrote = "Garrote";
        const string Stealth = "Stealth";
        const string CheapShot = "Cheap Shot";
        const string Ambush = "Ambush";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        bool SwapDaggerReady;
        bool DaggerEquipped;
        bool SwapMaceOrSwordReady;
        bool MaceOrSwordEquipped;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.botTasks = botTasks;
            this.container = container;
            this.target = target;
            player = ObjectManager.Instance.Player;
            stuckHelper = new StuckHelper(container, botTasks);
        }

        public void Update()
        {
            if (target.TappedByOther)
            {
                player.StopAllMovement();
                botTasks.Pop();
                return;
            }

            stuckHelper.CheckIfStuck();

            var distanceToTarget = player.Location.GetDistanceTo(target.Location);
            if (distanceToTarget < 30 && !player.HasBuff(Stealth) && Spellbook.Instance.IsSpellReady(Garrote) && !player.IsInCombat)
                Lua.Instance.Execute($"CastSpellByName('{Stealth}')");

            // Weapon Swap Logic
            ThreadSynchronizer.Instance.Invoke(() =>
            {                                             

                WoWItem MainHand = Inventory.Instance.GetEquippedItem(EquipSlot.MainHand);
                WoWItem OffHand = Inventory.Instance.GetEquippedItem(EquipSlot.OffHand);
                WoWItem SwapSlotWeap = Inventory.Instance.GetItem(4, 1);

                //Console.WriteLineVerbose("Mainhand Item Type:  " + MainHand.Info.ItemSubclass);
                //Console.WriteLineVerbose("Offhand Item Type:  " + OffHand.Info.ItemSubclass);
                //Console.WriteLineVerbose("Swap Weapon Item Type:  " + SwapSlotWeap.Info.ItemSubclass);
                //Console.WriteLineVerbose("Swap Weapon Item Type:  " + SwapSlotWeap.Info.Name);

                // Check to see if a Dagger is Equipped in the mainhand

                if (MainHand.Info.ItemClass == ItemClass.Dagger)

                    DaggerEquipped = true;

                else DaggerEquipped = false;

                // Check to see if a 1H Sword or Mace is Equipped in the mainhand

                // if (MainHand.Info.ItemSubclass == ItemSubclass.OneHandedMace || ItemSubclass.OneHandedSword || ItemSubclass.OneHandedExotic)
                if (MainHand.Info.ItemClass == ItemClass.SwordOneHand)

                    MaceOrSwordEquipped = true;

                else MaceOrSwordEquipped = false;

                // Check to see if a Dagger is ready in the swap slot

                if (SwapSlotWeap.Info.ItemClass == ItemClass.Dagger)

                    SwapDaggerReady = true;

                else SwapDaggerReady = false;


                // Check to see if a Sword, 1H Mace, or fist weapon is ready in the swap slot

                // if (SwapSlotWeap.Info.ItemSubclass == ItemSubclass.OneHandedMace || ItemSubclass.OneHandedSword || ItemSubclass.OneHandedExotic)
                if (SwapSlotWeap.Info.ItemClass == ItemClass.SwordOneHand)

                    SwapMaceOrSwordReady = true;

                else SwapMaceOrSwordReady = false;

                // If there is a mace or swap in the swap slot, the player swap back to the 1H sword or mace.

                // if (SwapMaceOrSwordReady == true)
                // {
                //    Lua.Instance.Execute($"UseContainerItem({4}, {2})");
                //    Console.WriteLineVerbose(MainHand.Info.Name + "Swapped Into Mainhand!");
                //}

                // If Swap dagger is ready and the playe is not in combat, swap to a mainhand dagger.

                if (SwapDaggerReady == true && !player.IsInCombat && !player.HasBuff(Stealth))
                {
                    Lua.Instance.Execute($"UseContainerItem({4}, {2})");
                    Console.WriteLine(MainHand.Info.Name + " swapped Into Mainhand!");
                }

            });


            if (distanceToTarget < 25 && Spellbook.Instance.IsSpellReady(Distract) && Spellbook.Instance.IsSpellReady(Distract) && player.HasBuff(Stealth))
            {
                //var delta = target.Location - player.Location;
                //var normalizedVector = delta.GetNormalizedVector();
                //var scaledVector = normalizedVector * 5;
                //var targetLocation = target.Location + scaledVector;

                //Functions.CastSpellAtPosition(Distract, targetLocation);
            }

            if (distanceToTarget < 5 && player.HasBuff(Stealth) && Spellbook.Instance.IsSpellReady(Ambush) && DaggerEquipped && !player.IsInCombat && player.IsBehind(target))
            {
                Lua.Instance.Execute($"CastSpellByName('{Ambush}')");
                return;
            }

            if (distanceToTarget < 5 && player.HasBuff(Stealth) && Spellbook.Instance.IsSpellReady(Garrote) && !DaggerEquipped && !player.IsInCombat && player.IsBehind(target))
            {
                Lua.Instance.Execute($"CastSpellByName('{Garrote}')");
                return;
            }

            if (distanceToTarget < 5 && player.HasBuff(Stealth) && Spellbook.Instance.IsSpellReady(CheapShot) && !player.IsInCombat && !player.IsBehind(target))
            {
                Lua.Instance.Execute($"CastSpellByName('{CheapShot}')");
                return;
            }

            if (distanceToTarget < 2.5)
            {
                player.StopAllMovement();
                botTasks.Pop();
                botTasks.Push(new PvERotationTask(container, botTasks));
                return;
            }

            var nextWaypoint = SocketClient.Instance.CalculatePath(player.MapId, player.Location, target.Location, false);
            player.MoveToward(nextWaypoint[0]);
        }
    }
}


