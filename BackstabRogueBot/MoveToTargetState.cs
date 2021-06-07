// Nat owns this file!

using BloogBot.AI;
using BloogBot.Game.Objects;
using BloogBot.Game.Enums;
using System.Collections.Generic;
using BloogBot;
using BloogBot.Game;

namespace BackstabRogueBot
{
    class MoveToTargetState : IBotState
    {
        const string Distract = "Distract";
        const string Garrote = "Garrote";
        const string Stealth = "Stealth";
        const string CheapShot = "Cheap Shot";
        const string Ambush = "Ambush";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        bool SwapDaggerReady;
        bool DaggerEquipped;
        bool SwapMaceOrSwordReady;
        bool MaceOrSwordEquipped;

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
            if (distanceToTarget < 30 && !player.HasBuff(Stealth) && player.KnowsSpell(Garrote) && !player.IsInCombat)
                player.LuaCall($"CastSpellByName('{Stealth}')");

            // Weapon Swap Logic
            ThreadSynchronizer.RunOnMainThread(() =>
            {                                             

                WoWItem MainHand = Inventory.GetEquippedItem(EquipSlot.MainHand);
                WoWItem OffHand = Inventory.GetEquippedItem(EquipSlot.OffHand);
                WoWItem SwapSlotWeap = Inventory.GetItem(4, 1);

                //Logger.LogVerbose("Mainhand Item Type:  " + MainHand.Info.ItemSubclass);
                //Logger.LogVerbose("Offhand Item Type:  " + OffHand.Info.ItemSubclass);
                //Logger.LogVerbose("Swap Weapon Item Type:  " + SwapSlotWeap.Info.ItemSubclass);
                //Logger.LogVerbose("Swap Weapon Item Type:  " + SwapSlotWeap.Info.Name);

                // Check to see if a Dagger is Equipped in the mainhand

                if (MainHand.Info.ItemSubclass == ItemSubclass.Dagger)

                    DaggerEquipped = true;

                else DaggerEquipped = false;

                // Check to see if a 1H Sword or Mace is Equipped in the mainhand

                // if (MainHand.Info.ItemSubclass == ItemSubclass.OneHandedMace || ItemSubclass.OneHandedSword || ItemSubclass.OneHandedExotic)
                if (MainHand.Info.ItemSubclass == ItemSubclass.OneHandedSword)

                    MaceOrSwordEquipped = true;

                else MaceOrSwordEquipped = false;

                // Check to see if a Dagger is ready in the swap slot

                if (SwapSlotWeap.Info.ItemSubclass == ItemSubclass.Dagger)

                    SwapDaggerReady = true;

                else SwapDaggerReady = false;


                // Check to see if a Sword, 1H Mace, or fist weapon is ready in the swap slot

                // if (SwapSlotWeap.Info.ItemSubclass == ItemSubclass.OneHandedMace || ItemSubclass.OneHandedSword || ItemSubclass.OneHandedExotic)
                if (SwapSlotWeap.Info.ItemSubclass == ItemSubclass.OneHandedSword)

                    SwapMaceOrSwordReady = true;

                else SwapMaceOrSwordReady = false;

                // If there is a mace or swap in the swap slot, the player swap back to the 1H sword or mace.

                // if (SwapMaceOrSwordReady == true)
                // {
                //    player.LuaCall($"UseContainerItem({4}, {2})");
                //    Logger.LogVerbose(MainHand.Info.Name + "Swapped Into Mainhand!");
                //}

                // If Swap dagger is ready and the playe is not in combat, swap to a mainhand dagger.

                if (SwapDaggerReady == true && !player.IsInCombat && !player.HasBuff(Stealth))
                {
                    player.LuaCall($"UseContainerItem({4}, {2})");
                    Logger.LogVerbose(MainHand.Info.Name + " swapped Into Mainhand!");
                }

            });


            if (distanceToTarget < 25 && player.KnowsSpell(Distract) && player.IsSpellReady(Distract) && player.HasBuff(Stealth))
            {
                var delta = target.Position - player.Position;
                var normalizedVector = delta.GetNormalizedVector();
                var scaledVector = normalizedVector * 5;
                var targetPosition = target.Position + scaledVector;

                player.CastSpellAtPosition(Distract, targetPosition);
            }

            if (distanceToTarget < 5 && player.HasBuff(Stealth) && player.IsSpellReady(Ambush) && DaggerEquipped && !player.IsInCombat && player.IsBehind(target))
            {
                player.LuaCall($"CastSpellByName('{Ambush}')");
                return;
            }

            if (distanceToTarget < 5 && player.HasBuff(Stealth) && player.IsSpellReady(Garrote) && !DaggerEquipped && !player.IsInCombat && player.IsBehind(target))
            {
                player.LuaCall($"CastSpellByName('{Garrote}')");
                return;
            }

            if (distanceToTarget < 5 && player.HasBuff(Stealth) && player.IsSpellReady(CheapShot) && !player.IsInCombat && !player.IsBehind(target))
            {
                player.LuaCall($"CastSpellByName('{CheapShot}')");
                return;
            }

            if (distanceToTarget < 2.5)
            {
                player.StopAllMovement();
                botStates.Pop();
                botStates.Push(new CombatState(botStates, container, target));
                return;
            }

            var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
            player.MoveToward(nextWaypoint);
        }
    }
}


