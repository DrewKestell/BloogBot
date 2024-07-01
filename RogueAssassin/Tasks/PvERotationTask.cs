// Nat owns this file!

using WoWActivityMember;
using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;
using WoWActivityMember.Tasks;
using WoWActivityMember.Tasks.SharedStates;
using static WoWActivityMember.Constants.Enums;

namespace RogueAssassin.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {
        private const string AdrenalineRush = "Adrenaline Rush";
        private const string BladeFlurry = "Blade Flurry";
        private const string Evasion = "Evasion";
        private const string Eviscerate = "Eviscerate";
        private const string Gouge = "Gouge";
        private const string BloodFury = "Blood Fury";
        private const string Kick = "Kick";
        private const string Riposte = "Riposte";
        private const string SinisterStrike = "Sinister Strike";
        private const string SliceAndDice = "Slice and Dice";
        private const string GhostlyStrike = "Ghostly Strike";
        private const string Blind = "Blind";
        private const string KidneyShot = "Kidney Shot";
        private const string ExposeArmor = "Expose Armor";
        private WoWUnit secondaryTarget;
        private bool SwapDaggerReady;
        private bool DaggerEquipped;
        private bool SwapMaceOrSwordReady;
        private bool MaceOrSwordEquipped;
        private bool readyToRiposte;
        private int riposteStartTime;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            WoWEventHandler.Instance.OnParry += OnParryCallback;
        }

        ~PvERotationTask()
        {
            WoWEventHandler.Instance.OnParry -= OnParryCallback;
        }

        public void Update()
        {
            if (Environment.TickCount - riposteStartTime > 5000 && readyToRiposte)
                readyToRiposte = false;

            
            if (ObjectManager.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Target == null || ObjectManager.Player.Target.HealthPercent <= 0)
            {
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors.First().Guid);
            }

            if (Update(3))
                return;

            // Ensure Sword/Mace/1H is equipped (not dagger)
            
            ThreadSynchronizer.RunOnMainThread(() =>
            {

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

                if (SwapMaceOrSwordReady == true)
                {
                    Functions.LuaCall($"UseContainerItem({4}, {2})");
                    Console.WriteLine(MainHand.Info.Name + "Swapped Into Mainhand!");
                }
            });

            // set secondaryTarget
            // if (ObjectManager.Aggressors.Count() == 2 && secondaryTarget == null)
            //    secondaryTarget = ObjectManager.Aggressors.Single(u => u.Guid != ObjectManager.Player.TargetGuid);

            //if (secondaryTarget != null && !secondaryTarget.HasDebuff(Blind))
            // {
            //    ObjectManager.Player.SetTarget(secondaryTarget.Guid);
            //     TryUseAbility(Blind, 30, ObjectManager.Player.IsSpellReady(Blind) && !secondaryTarget.HasDebuff(Blind));
            // }

            // ----- COMBAT ROTATION -----

            bool readyToEviscerate =
                ObjectManager.Player.Target.HealthPercent <= 20 && ObjectManager.Player.ComboPoints >= 2
                || ObjectManager.Player.Target.HealthPercent <= 30 && ObjectManager.Player.ComboPoints >= 3
                || ObjectManager.Player.Target.HealthPercent <= 40 && ObjectManager.Player.ComboPoints >= 4
                || ObjectManager.Player.ComboPoints == 5;
            
            TryUseAbility(Eviscerate, 35, readyToEviscerate);

            TryUseAbility(SliceAndDice, 25, !ObjectManager.Player.HasBuff(SliceAndDice) && ObjectManager.Player.Target.HealthPercent > 40 && ObjectManager.Player.ComboPoints <= 3 && ObjectManager.Player.ComboPoints >= 2);

            // TryUseAbility(ExposeArmor, 25, ObjectManager.Player.HasBuff(SliceAndDice) && ObjectManager.Player.Target.HealthPercent > 50 && ObjectManager.Player.ComboPoints <= 2 && ObjectManager.Player.ComboPoints >= 1);

            TryUseAbility(SinisterStrike, 45, !ObjectManager.Player.IsSpellReady(GhostlyStrike) && !ReadyToInterrupt() && ObjectManager.Player.ComboPoints < 5 && !readyToEviscerate);
        
            TryUseAbility(GhostlyStrike, 40, ObjectManager.Player.IsSpellReady(GhostlyStrike) && ObjectManager.Player.IsSpellReady(GhostlyStrike) && !ReadyToInterrupt() && ObjectManager.Player.ComboPoints < 5 && !readyToEviscerate);

            TryUseAbilityById(BloodFury, 3, 0, ObjectManager.Player.IsSpellReady(BloodFury) && ObjectManager.Player.Target.HealthPercent > 80);

            TryUseAbility(Evasion, 0, ObjectManager.Aggressors.Count() > 1);

            TryUseAbility(BladeFlurry, 25, ObjectManager.Aggressors.Count() > 1);

            TryUseAbility(Riposte, 10, readyToRiposte, RiposteCallback);

            // Caster interrupt abilities

            TryUseAbility(Kick, 25, ReadyToInterrupt());

            // we use Kidneyshot (with 1 or 2 combo points only) before Gouge as Gouge has a longer cooldown and requires more energy, so sometimes gouge doesn't fire before casting is done.
            
            TryUseAbility(KidneyShot, 25, ReadyToInterrupt() && !ObjectManager.Player.IsSpellReady(Kick) && ObjectManager.Player.ComboPoints >= 1 && ObjectManager.Player.ComboPoints <=2);
                        
            TryUseAbility(Gouge, 45, ReadyToInterrupt() && !ObjectManager.Player.IsSpellReady(Kick));
        }

        private void OnParryCallback(object sender, EventArgs e)
        {
            readyToRiposte = true;
            riposteStartTime = Environment.TickCount;
        }

        private bool ReadyToInterrupt() => ObjectManager.Player.Target.Mana > 0 && (ObjectManager.Player.Target.IsCasting || ObjectManager.Player.Target.IsChanneling);

        public override void PerformCombatRotation()
        {
            throw new NotImplementedException();
        }

        private Action RiposteCallback => () => readyToRiposte = false;

        public object ItemSubclass { get; private set; }
    }
}
