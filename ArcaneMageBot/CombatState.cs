using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcaneMageBot
{
    class CombatState : CombatStateBase, IBotState
    {
        const string WandLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Shoot') end";

        const string ArcaneMissiles = "Arcane Missiles";
        const string ArcanePower = "Arcane Power";
        const string Clearcasting = "Clearcasting";
        const string Counterspell = "Counterspell";
        const string FrostNova = "Frost Nova";
        const string Fireball = "Fireball";
        const string FireBlast = "Fire Blast";
        const string ManaShield = "Mana Shield";
        const string PresenceOfMind = "Presence of Mind";

        readonly LocalPlayer player;
        readonly WoWUnit target;

        bool frostNovaBackpedaling;
        int frostNovaBackpedalStartTime;

        internal CombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) : base(botStates, container, target, 30)
        {
            player = ObjectManager.Player;
            this.target = target;
        }

        public new void Update()
        {
            if (frostNovaBackpedaling && Environment.TickCount - frostNovaBackpedalStartTime > 1500)
            {
                player.StopMovement(ControlBits.Back);
                frostNovaBackpedaling = false;
            }
            if (frostNovaBackpedaling)
                return;

            if (base.Update())
                return;

            var hasWand = Inventory.GetEquippedItem(EquipSlot.Ranged) != null;
            var useWand = hasWand && player.ManaPercent <= 10 && !player.IsCasting && !player.IsChanneling;
            if (useWand)
                player.LuaCall(WandLuaScript);

            TryCastSpell(PresenceOfMind, 0, 50, target.HealthPercent > 80);

            TryCastSpell(ArcanePower, 0, 50, target.HealthPercent > 80);

            TryCastSpell(Counterspell, 0, 29, target.Mana > 0 && target.IsCasting);

            TryCastSpell(ManaShield, 0, 50, (!player.HasBuff(ManaShield) && player.HealthPercent < 20));

            TryCastSpell(FireBlast, 0, 19, !player.HasBuff(Clearcasting));

            TryCastSpell(FrostNova, 0, 10, !ObjectManager.Units.Any(u => u.Guid != target.Guid && u.Health > 0 && u.Position.DistanceTo(player.Position) < 15), callback: FrostNovaCallback);

            TryCastSpell(Fireball, 0, 34, player.Level < 15 || player.HasBuff(PresenceOfMind));

            TryCastSpell(ArcaneMissiles, 0, 29, player.Level >= 15);
        }

        Action FrostNovaCallback => () =>
        {
            frostNovaBackpedaling = true;
            frostNovaBackpedalStartTime = Environment.TickCount;
            player.StartMovement(ControlBits.Back);
        };
    }
}
