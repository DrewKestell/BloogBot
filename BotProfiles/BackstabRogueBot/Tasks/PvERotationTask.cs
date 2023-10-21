// Nat owns this file!

using RaidMemberBot;
using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace BackstabRogueBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
    {
        const string AdrenalineRush = "Adrenaline Rush";
        const string BladeFlurry = "Blade Flurry";
        const string Evasion = "Evasion";
        const string Eviscerate = "Eviscerate";
        const string Gouge = "Gouge";
        const string BloodFury = "Blood Fury";
        const string Kick = "Kick";
        const string Riposte = "Riposte";
        const string SinisterStrike = "Sinister Strike";
        const string SliceAndDice = "Slice and Dice";
        const string GhostlyStrike = "Ghostly Strike";
        const string Blind = "Blind";
        const string KidneyShot = "Kidney Shot";
        const string ExposeArmor = "Expose Armor";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;
        WoWUnit target;
        WoWUnit secondaryTarget;
        
        bool SwapDaggerReady;
        bool DaggerEquipped;
        bool SwapMaceOrSwordReady;
        bool MaceOrSwordEquipped;

        bool readyToRiposte;
        int riposteStartTime;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;

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

            
            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (target == null || Container.HostileTarget.HealthPercent <= 0)
            {
                target = ObjectManager.Instance.Aggressors.First();
            }

            if (Update(target, 3))
                return;

            // Ensure Sword/Mace/1H is equipped (not dagger)
            
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

                if (SwapMaceOrSwordReady == true)
                {
                    Lua.Instance.Execute($"UseContainerItem({4}, {2})");
                    Console.WriteLine(MainHand.Info.Name + "Swapped Into Mainhand!");
                }
            });

            // set secondaryTarget
            // if (ObjectManager.Instance.Aggressors.Count() == 2 && secondaryTarget == null)
            //    secondaryTarget = ObjectManager.Instance.Aggressors.Single(u => u.Guid != Container.HostileTarget.Guid);

            //if (secondaryTarget != null && !secondaryTarget.HasDebuff(Blind))
            // {
            //    Container.Player.SetTarget(secondaryTarget.Guid);
            //     TryUseAbility(Blind, 30, Spellbook.Instance.IsSpellReady(Blind) && !secondaryTarget.HasDebuff(Blind));
            // }

            // ----- COMBAT ROTATION -----

            var readyToEviscerate =
                Container.HostileTarget.HealthPercent <= 20 && Container.Player.ComboPoints >= 2
                || Container.HostileTarget.HealthPercent <= 30 && Container.Player.ComboPoints >= 3
                || Container.HostileTarget.HealthPercent <= 40 && Container.Player.ComboPoints >= 4
                || Container.Player.ComboPoints == 5;
            
            TryUseAbility(Eviscerate, 35, readyToEviscerate);

            TryUseAbility(SliceAndDice, 25, !Container.Player.HasBuff(SliceAndDice) && Container.HostileTarget.HealthPercent > 40 && Container.Player.ComboPoints <= 3 && Container.Player.ComboPoints >= 2);

            // TryUseAbility(ExposeArmor, 25, Container.Player.HasBuff(SliceAndDice) && Container.HostileTarget.HealthPercent > 50 && Container.Player.ComboPoints <= 2 && Container.Player.ComboPoints >= 1);

            TryUseAbility(SinisterStrike, 45, !Spellbook.Instance.IsSpellReady(GhostlyStrike) && !ReadyToInterrupt(target) && Container.Player.ComboPoints < 5 && !readyToEviscerate);
        
            TryUseAbility(GhostlyStrike, 40, Spellbook.Instance.IsSpellReady(GhostlyStrike) && Spellbook.Instance.IsSpellReady(GhostlyStrike) && !ReadyToInterrupt(target) && Container.Player.ComboPoints < 5 && !readyToEviscerate);

            TryUseAbilityById(BloodFury, 3, 0, Spellbook.Instance.IsSpellReady(BloodFury) && Container.HostileTarget.HealthPercent > 80);

            TryUseAbility(Evasion, 0, ObjectManager.Instance.Aggressors.Count() > 1);

            TryUseAbility(BladeFlurry, 25, ObjectManager.Instance.Aggressors.Count() > 1);

            TryUseAbility(Riposte, 10, readyToRiposte, RiposteCallback);

            // Caster interrupt abilities

            TryUseAbility(Kick, 25, ReadyToInterrupt(target));

            // we use Kidneyshot (with 1 or 2 combo points only) before Gouge as Gouge has a longer cooldown and requires more energy, so sometimes gouge doesn't fire before casting is done.
            
            TryUseAbility(KidneyShot, 25, ReadyToInterrupt(target) && !Spellbook.Instance.IsSpellReady(Kick) && Container.Player.ComboPoints >= 1 && Container.Player.ComboPoints <=2);
                        
            TryUseAbility(Gouge, 45, ReadyToInterrupt(target) && !Spellbook.Instance.IsSpellReady(Kick));
        }

        void OnParryCallback(object sender, EventArgs e)
        {
            readyToRiposte = true;
            riposteStartTime = Environment.TickCount;
        }

        bool ReadyToInterrupt(WoWUnit target) => Container.HostileTarget.Mana > 0 && (target.IsCasting || Container.HostileTarget.IsChanneling);

        Action RiposteCallback => () => readyToRiposte = false;

        public object ItemSubclass { get; private set; }
    }
}
