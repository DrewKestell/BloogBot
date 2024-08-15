using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;
using static BotRunner.Constants.Spellbook;

namespace RogueAssassin.Tasks
{
    internal class PullTargetTask : BotTask, IBotTask
    {
        private bool SwapDaggerReady;
        private bool DaggerEquipped;
        private bool SwapMaceOrSwordReady;
        private bool MaceOrSwordEquipped;

        internal PullTargetTask(IBotContext botContext) : base(botContext) { }

        public void Update()
        {
            if (ObjectManager.GetTarget(ObjectManager.Player).TappedByOther)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(ObjectManager.GetTarget(ObjectManager.Player).Position);
            if (distanceToTarget < 30 && !ObjectManager.Player.HasBuff(Stealth) && ObjectManager.Player.IsSpellReady(Garrote) && !ObjectManager.Player.IsInCombat)
                ObjectManager.Player.CastSpell(Stealth);

            // Weapon Swap Logic                                       

            IWoWItem MainHand = ObjectManager.GetEquippedItem(EquipSlot.MainHand);
            IWoWItem OffHand = ObjectManager.GetEquippedItem(EquipSlot.OffHand);
            IWoWItem SwapSlotWeap = ObjectManager.GetItem(4, 1);

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
            //    Functions.LuaCall($"UseContainerItem({4}, {2})");
            //    Console.WriteLineVerbose(MainHand.Info.Name + "Swapped Into Mainhand!");
            //}

            // If Swap dagger is ready and the playe is not in combat, swap to a mainhand dagger.

            if (SwapDaggerReady == true && !ObjectManager.Player.IsInCombat && !ObjectManager.Player.HasBuff(Stealth))
            {
                ObjectManager.UseContainerItem(4, 2);
                Console.WriteLine(MainHand.Info.Name + " swapped Into Mainhand!");
            }


            if (distanceToTarget < 25 && ObjectManager.Player.IsSpellReady(Distract) && ObjectManager.Player.IsSpellReady(Distract) && ObjectManager.Player.HasBuff(Stealth))
            {
                //var delta = ObjectManager.GetTarget(ObjectManager.Player).Position - ObjectManager.Player.Position;
                //var normalizedVector = delta.GetNormalizedVector();
                //var scaledVector = normalizedVector * 5;
                //var targetPosition = ObjectManager.GetTarget(ObjectManager.Player).Position + scaledVector;

                //Functions.CastSpellAtPosition(Distract, targe.Position);
            }

            if (distanceToTarget < 5 && ObjectManager.Player.HasBuff(Stealth) && ObjectManager.Player.IsSpellReady(Ambush) && DaggerEquipped && !ObjectManager.Player.IsInCombat && ObjectManager.Player.IsBehind(ObjectManager.GetTarget(ObjectManager.Player)))
            {
                ObjectManager.Player.CastSpell(Ambush);
                return;
            }

            if (distanceToTarget < 5 && ObjectManager.Player.HasBuff(Stealth) && ObjectManager.Player.IsSpellReady(Garrote) && !DaggerEquipped && !ObjectManager.Player.IsInCombat && ObjectManager.Player.IsBehind(ObjectManager.GetTarget(ObjectManager.Player)))
            {
                ObjectManager.Player.CastSpell(Garrote);
                return;
            }

            if (distanceToTarget < 5 && ObjectManager.Player.HasBuff(Stealth) && ObjectManager.Player.IsSpellReady(CheapShot) && !ObjectManager.Player.IsInCombat && !ObjectManager.Player.IsBehind(ObjectManager.GetTarget(ObjectManager.Player)))
            {
                ObjectManager.Player.CastSpell(CheapShot);
                return;
            }

            if (distanceToTarget < 2.5)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(BotContext));
                return;
            }

            Position[] nextWaypoint = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.GetTarget(ObjectManager.Player).Position, true);
            ObjectManager.Player.MoveToward(nextWaypoint[0]);
        }
    }
}


