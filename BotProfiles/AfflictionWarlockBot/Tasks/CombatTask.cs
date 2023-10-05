using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace AfflictionWarlockBot
{
    class CombatTask : CombatRotationTask, IBotTask
    {
        const string WandLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Shoot') end";
        const string TurnOffWandLuaScript = "if IsAutoRepeatAction(11) ~= nil then CastSpellByName('Shoot') end";

        const string Corruption = "Corruption";
        const string CurseOfAgony = "Curse of Agony";
        const string DeathCoil = "Death Coil";
        const string DrainSoul = "Drain Soul";
        const string Immolate = "Immolate";
        const string LifeTap = "Life Tap";
        const string ShadowBolt = "Shadow Bolt";
        const string SiphonLife = "Siphon Life";

        readonly IClassContainer container;
        readonly LocalPlayer player;
        readonly List<WoWUnit> targets;

        internal CombatTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> targets) : base(container, botTasks, targets, 30)
        {
            this.container = container;
            player = ObjectManager.Instance.Player;
            this.targets = targets;
        }

        public new void Update()
        {
            if (base.Update())
                return;

            ObjectManager.Instance.Pet?.Attack();

            // if target is low on health, turn off wand and cast drain soul
            if (targets[0].HealthPercent <= 20)
            {
                Lua.Instance.Execute(TurnOffWandLuaScript);
                TryCastSpell(DrainSoul, 0, 29);
            }

            var wand = Inventory.Instance.GetEquippedItem(EquipSlot.Ranged);
            if (wand != null && (player.ManaPercent <= 10 || targets[0].HealthPercent <= 60 && targets[0].HealthPercent > 20 && player.Channeling == 0 && player.Casting == 0))
                Lua.Instance.Execute(WandLuaScript);
            else
            {
                TryCastSpell(DeathCoil, 0, 30, (targets[0].Casting > 0 || targets[0].Channeling > 0) && targets[0].HealthPercent > 20);

                TryCastSpell(LifeTap, 0, int.MaxValue, player.HealthPercent > 85 && player.ManaPercent < 80);

                TryCastSpell(CurseOfAgony, 0, 30, !targets[0].GotDebuff(CurseOfAgony) && targets[0].HealthPercent > 90);

                TryCastSpell(Immolate, 0, 30, !targets[0].GotDebuff(Immolate) && targets[0].HealthPercent > 30);

                TryCastSpell(Corruption, 0, 30, !targets[0].GotDebuff(Corruption) && targets[0].HealthPercent > 30);

                TryCastSpell(SiphonLife, 0, 30, !targets[0].GotDebuff(SiphonLife) && targets[0].HealthPercent > 50);

                TryCastSpell(ShadowBolt, 0, 30, targets[0].HealthPercent > 40 || wand == null);
            }
        }
    }
}
