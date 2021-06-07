using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace AfflictionWarlockBot
{
    class CombatState : CombatStateBase, IBotState
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

        readonly LocalPlayer player;
        readonly WoWUnit target;

        internal CombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) : base(botStates, container, target, 30)
        {
            player = ObjectManager.Player;
            this.target = target;
        }

        public new void Update()
        {
            if (base.Update())
                return;

            ObjectManager.Pet?.Attack();

            // if target is low on health, turn off wand and cast drain soul
            if (target.HealthPercent <= 20)
            {
                player.LuaCall(TurnOffWandLuaScript);
                TryCastSpell(DrainSoul, 0, 29);
            }

            var wand = Inventory.GetEquippedItem(EquipSlot.Ranged);
            if (wand != null && (player.ManaPercent <= 10 || (target.HealthPercent <= 60 && target.HealthPercent > 20) && !player.IsChanneling && !player.IsCasting))
                player.LuaCall(WandLuaScript);
            else
            {
                TryCastSpell(DeathCoil, 0, 30, (target.IsCasting || target.IsChanneling) && target.HealthPercent > 20);

                TryCastSpell(LifeTap, 0, int.MaxValue, player.HealthPercent > 85 && player.ManaPercent < 80);

                TryCastSpell(CurseOfAgony, 0, 30, !target.HasDebuff(CurseOfAgony) && target.HealthPercent > 90);

                TryCastSpell(Immolate, 0, 30, !target.HasDebuff(Immolate) && target.HealthPercent > 30);

                TryCastSpell(Corruption, 0, 30, !target.HasDebuff(Corruption) && target.HealthPercent > 30);

                TryCastSpell(SiphonLife, 0, 30, !target.HasDebuff(SiphonLife) && target.HealthPercent > 50);

                TryCastSpell(ShadowBolt, 0, 30, target.HealthPercent > 40 || wand == null);
            }
        }
    }
}
