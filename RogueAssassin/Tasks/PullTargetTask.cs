// Nat owns this file!

using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;
using WoWActivityMember.Tasks;
using static WoWActivityMember.Constants.Enums;

namespace BackstabRogueBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string Distract = "Distract";
        const string Garrote = "Garrote";
        const string Stealth = "Stealth";
        const string CheapShot = "Cheap Shot";
        const string Ambush = "Ambush";

        bool SwapDaggerReady;
        bool DaggerEquipped;
        bool SwapMaceOrSwordReady;
        bool MaceOrSwordEquipped;

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull) { }

        public void Update()
        {
            if (ObjectManager.Player.Target.TappedByOther)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position);
            if (distanceToTarget < 30 && !ObjectManager.Player.HasBuff(Stealth) && ObjectManager.Player.IsSpellReady(Garrote) && !ObjectManager.Player.IsInCombat)
                Functions.LuaCall($"CastSpellByName('{Stealth}')");

            // Weapon Swap Logic                                       

            WoWItem MainHand = Inventory.GetEquippedItem(EquipSlot.MainHand);
            WoWItem OffHand = Inventory.GetEquippedItem(EquipSlot.OffHand);
            WoWItem SwapSlotWeap = Inventory.GetItem(4, 1);

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
                Functions.LuaCall($"UseContainerItem({4}, {2})");
                Console.WriteLine(MainHand.Info.Name + " swapped Into Mainhand!");
            }


            if (distanceToTarget < 25 && ObjectManager.Player.IsSpellReady(Distract) && ObjectManager.Player.IsSpellReady(Distract) && ObjectManager.Player.HasBuff(Stealth))
            {
                //var delta = ObjectManager.Player.Target.Position - ObjectManager.Player.Position;
                //var normalizedVector = delta.GetNormalizedVector();
                //var scaledVector = normalizedVector * 5;
                //var targetPosition = ObjectManager.Player.Target.Position + scaledVector;

                //Functions.CastSpellAtPosition(Distract, targe.Position);
            }

            if (distanceToTarget < 5 && ObjectManager.Player.HasBuff(Stealth) && ObjectManager.Player.IsSpellReady(Ambush) && DaggerEquipped && !ObjectManager.Player.IsInCombat && ObjectManager.Player.IsBehind(ObjectManager.Player.Target))
            {
                Functions.LuaCall($"CastSpellByName('{Ambush}')");
                return;
            }

            if (distanceToTarget < 5 && ObjectManager.Player.HasBuff(Stealth) && ObjectManager.Player.IsSpellReady(Garrote) && !DaggerEquipped && !ObjectManager.Player.IsInCombat && ObjectManager.Player.IsBehind(ObjectManager.Player.Target))
            {
                Functions.LuaCall($"CastSpellByName('{Garrote}')");
                return;
            }

            if (distanceToTarget < 5 && ObjectManager.Player.HasBuff(Stealth) && ObjectManager.Player.IsSpellReady(CheapShot) && !ObjectManager.Player.IsInCombat && !ObjectManager.Player.IsBehind(ObjectManager.Player.Target))
            {
                Functions.LuaCall($"CastSpellByName('{CheapShot}')");
                return;
            }

            if (distanceToTarget < 2.5)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(Container, BotTasks));
                return;
            }

            Position[] nextWaypoint = Navigation.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);
            ObjectManager.Player.MoveToward(nextWaypoint[0]);
        }
    }
}


