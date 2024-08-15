using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace RogueAssassin.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {
        private IWoWUnit secondaryTarget;
        private bool SwapDaggerReady;
        private bool DaggerEquipped;
        private bool SwapMaceOrSwordReady;
        private bool MaceOrSwordEquipped;
        private bool readyToRiposte;
        private int riposteStartTime;

        internal PvERotationTask(IBotContext botContext) : base(botContext) => EventHandler.OnParry += OnParryCallback;

        ~PvERotationTask()
        {
            EventHandler.OnParry -= OnParryCallback;
        }

        public void Update()
        {
            if (Environment.TickCount - riposteStartTime > 5000 && readyToRiposte)
                readyToRiposte = false;

            
            if (!ObjectManager.Aggressors.Any())
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.GetTarget(ObjectManager.Player) == null || ObjectManager.GetTarget(ObjectManager.Player).HealthPercent <= 0)
            {
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors.First().Guid);
            }

            if (Update(3))
                return;

            // Ensure Sword/Mace/1H is equipped (not dagger)
            
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

                if (SwapMaceOrSwordReady == true)
                {
                    ObjectManager.UseContainerItem(4, 21);
                    Console.WriteLine(MainHand.Info.Name + "Swapped Into Mainhand!");
                }

            // set secondaryTarget
            // if (ObjectManager.Aggressors.Count() == 2 && secondaryTarget == null)
            //    secondaryTarget = ObjectManager.Aggressors.Single(u => u.Guid != ObjectManager.GetTarget(ObjectManager.Player).Guid);

            //if (secondaryTarget != null && !secondaryTarget.HasDebuff(Blind))
            // {
            //    ObjectManager.Player.SetTarget(secondaryTarget.Guid);
            //     TryUseAbility(Blind, 30, ObjectManager.Player.IsSpellReady(Blind) && !secondaryTarget.HasDebuff(Blind));
            // }

            // ----- COMBAT ROTATION -----

            bool readyToEviscerate =
                ObjectManager.GetTarget(ObjectManager.Player).HealthPercent <= 20 && ObjectManager.Player.ComboPoints >= 2
                || ObjectManager.GetTarget(ObjectManager.Player).HealthPercent <= 30 && ObjectManager.Player.ComboPoints >= 3
                || ObjectManager.GetTarget(ObjectManager.Player).HealthPercent <= 40 && ObjectManager.Player.ComboPoints >= 4
                || ObjectManager.Player.ComboPoints == 5;
            
            TryUseAbility(Eviscerate, 35, readyToEviscerate);

            TryUseAbility(SliceAndDice, 25, !ObjectManager.Player.HasBuff(SliceAndDice) && ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 40 && ObjectManager.Player.ComboPoints <= 3 && ObjectManager.Player.ComboPoints >= 2);

            // TryUseAbility(ExposeArmor, 25, ObjectManager.Player.HasBuff(SliceAndDice) && ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 50 && ObjectManager.Player.ComboPoints <= 2 && ObjectManager.Player.ComboPoints >= 1);

            TryUseAbility(SinisterStrike, 45, !ObjectManager.Player.IsSpellReady(GhostlyStrike) && !ReadyToInterrupt() && ObjectManager.Player.ComboPoints < 5 && !readyToEviscerate);
        
            TryUseAbility(GhostlyStrike, 40, ObjectManager.Player.IsSpellReady(GhostlyStrike) && ObjectManager.Player.IsSpellReady(GhostlyStrike) && !ReadyToInterrupt() && ObjectManager.Player.ComboPoints < 5 && !readyToEviscerate);

            TryUseAbilityById(BloodFury, 3, 0, ObjectManager.Player.IsSpellReady(BloodFury) && ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 80);

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

        private bool ReadyToInterrupt() => ObjectManager.GetTarget(ObjectManager.Player).Mana > 0 && (ObjectManager.GetTarget(ObjectManager.Player).IsCasting || ObjectManager.GetTarget(ObjectManager.Player).IsChanneling);

        public override void PerformCombatRotation()
        {
            throw new NotImplementedException();
        }

        private Action RiposteCallback => () => readyToRiposte = false;

        public object ItemSubclass { get; private set; }
    }
}
