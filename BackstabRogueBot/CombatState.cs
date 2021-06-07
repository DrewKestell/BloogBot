// Nat owns this file!

using BloogBot;
using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackstabRogueBot
{
    class CombatState : CombatStateBase, IBotState
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

        readonly LocalPlayer player;
        WoWUnit target;
        WoWUnit secondaryTarget;
        
        bool SwapDaggerReady;
        bool DaggerEquipped;
        bool SwapMaceOrSwordReady;
        bool MaceOrSwordEquipped;

        bool readyToRiposte;
        int riposteStartTime;

        internal CombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) : base(botStates, container, target, 3)
        {
            player = ObjectManager.Player;
            this.target = target;

            WoWEventHandler.OnParry += OnParryCallback;
        }

        ~CombatState()
        {
            WoWEventHandler.OnParry -= OnParryCallback;
        }

        public new void Update()
        {
            if (Environment.TickCount - riposteStartTime > 5000 && readyToRiposte)
                readyToRiposte = false;

            if (base.Update())
                return;

            // Ensure Sword/Mace/1H is equipped (not dagger)
            
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

                if (SwapMaceOrSwordReady == true)
                {
                    player.LuaCall($"UseContainerItem({4}, {2})");
                    Logger.LogVerbose(MainHand.Info.Name + "Swapped Into Mainhand!");
                }
            });

            // set secondaryTarget
            // if (ObjectManager.Aggressors.Count() == 2 && secondaryTarget == null)
            //    secondaryTarget = ObjectManager.Aggressors.Single(u => u.Guid != target.Guid);

            //if (secondaryTarget != null && !secondaryTarget.HasDebuff(Blind))
            // {
            //    player.SetTarget(secondaryTarget.Guid);
            //     TryUseAbility(Blind, 30, player.IsSpellReady(Blind) && !secondaryTarget.HasDebuff(Blind));
            // }

            // ----- COMBAT ROTATION -----

            var readyToEviscerate =
                target.HealthPercent <= 20 && player.ComboPoints >= 2
                || target.HealthPercent <= 30 && player.ComboPoints >= 3
                || target.HealthPercent <= 40 && player.ComboPoints >= 4
                || player.ComboPoints == 5;
            
            TryUseAbility(Eviscerate, 35, readyToEviscerate);

            TryUseAbility(SliceAndDice, 25, !player.HasBuff(SliceAndDice) && target.HealthPercent > 40 && player.ComboPoints <= 3 && player.ComboPoints >= 2);

            // TryUseAbility(ExposeArmor, 25, player.HasBuff(SliceAndDice) && target.HealthPercent > 50 && player.ComboPoints <= 2 && player.ComboPoints >= 1);

            TryUseAbility(SinisterStrike, 45, !player.IsSpellReady(GhostlyStrike) && !ReadyToInterrupt(target) && player.ComboPoints < 5 && !readyToEviscerate);
        
            TryUseAbility(GhostlyStrike, 40, player.IsSpellReady(GhostlyStrike) && player.KnowsSpell(GhostlyStrike) && !ReadyToInterrupt(target) && player.ComboPoints < 5 && !readyToEviscerate);

            TryUseAbilityById(BloodFury, 3, 0, player.IsSpellReady(BloodFury) && target.HealthPercent > 80);

            TryUseAbility(Evasion, 0, ObjectManager.Aggressors.Count() > 1);

            TryUseAbility(BladeFlurry, 25, ObjectManager.Aggressors.Count() > 1);

            TryUseAbility(Riposte, 10, readyToRiposte, RiposteCallback);

            // Caster interrupt abilities

            TryUseAbility(Kick, 25, ReadyToInterrupt(target));

            // we use Kidneyshot (with 1 or 2 combo points only) before Gouge as Gouge has a longer cooldown and requires more energy, so sometimes gouge doesn't fire before casting is done.
            
            TryUseAbility(KidneyShot, 25, ReadyToInterrupt(target) && !player.IsSpellReady(Kick) && player.ComboPoints >= 1 && player.ComboPoints <=2);
                        
            TryUseAbility(Gouge, 45, ReadyToInterrupt(target) && !player.IsSpellReady(Kick));
        }

        void OnParryCallback(object sender, EventArgs e)
        {
            readyToRiposte = true;
            riposteStartTime = Environment.TickCount;
        }

        bool ReadyToInterrupt(WoWUnit target) => target.Mana > 0 && (target.IsCasting || target.IsChanneling);

        Action RiposteCallback => () => readyToRiposte = false;
    }
}
