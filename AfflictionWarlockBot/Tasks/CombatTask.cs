using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;

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
            player = ObjectManager.Player;
            this.targets = targets;
        }

        public new void Update()
        {
            if (base.Update())
                return;

            ObjectManager.Pet?.Attack();

            // if target is low on health, turn off wand and cast drain soul
            if (targets[0].HealthPercent <= 20)
            {
                player.LuaCall(TurnOffWandLuaScript);
                TryCastSpell(DrainSoul, 0, 29);
            }

            var wand = Inventory.GetEquippedItem(EquipSlot.Ranged);
            if (wand != null && (player.ManaPercent <= 10 || targets[0].HealthPercent <= 60 && targets[0].HealthPercent > 20 && !player.IsChanneling && !player.IsCasting))
                player.LuaCall(WandLuaScript);
            else
            {
                TryCastSpell(DeathCoil, 0, 30, (targets[0].IsCasting || targets[0].IsChanneling) && targets[0].HealthPercent > 20);

                TryCastSpell(LifeTap, 0, int.MaxValue, player.HealthPercent > 85 && player.ManaPercent < 80);

                TryCastSpell(CurseOfAgony, 0, 30, !targets[0].HasDebuff(CurseOfAgony) && targets[0].HealthPercent > 90);

                TryCastSpell(Immolate, 0, 30, !targets[0].HasDebuff(Immolate) && targets[0].HealthPercent > 30);

                TryCastSpell(Corruption, 0, 30, !targets[0].HasDebuff(Corruption) && targets[0].HealthPercent > 30);

                TryCastSpell(SiphonLife, 0, 30, !targets[0].HasDebuff(SiphonLife) && targets[0].HealthPercent > 50);

                TryCastSpell(ShadowBolt, 0, 30, targets[0].HealthPercent > 40 || wand == null);
            }
        }
    }
}
