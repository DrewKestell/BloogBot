using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace ArcaneMageBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
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
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;

        bool frostNovaBackpedaling;
        int frostNovaBackpedalStartTime;
        WoWUnit target;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if (frostNovaBackpedaling && Environment.TickCount - frostNovaBackpedalStartTime > 1500)
            {
                Container.Player.StopMovement(ControlBits.Back);
                frostNovaBackpedaling = false;
            }
            if (frostNovaBackpedaling)
                return;


            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (target == null || Container.HostileTarget.HealthPercent <= 0)
            {
                target = ObjectManager.Instance.Aggressors[0];
            }

            if (Update(target, 30))
                return;

            bool hasWand = Inventory.Instance.GetEquippedItem(EquipSlot.Ranged) != null;
            bool useWand = hasWand && Container.Player.ManaPercent <= 10 && Container.Player.IsCasting && Container.Player.Channeling == 0;
            if (useWand)
                Lua.Instance.Execute(WandLuaScript);

            TryCastSpell(PresenceOfMind, 0, 50, Container.HostileTarget.HealthPercent > 80);

            TryCastSpell(ArcanePower, 0, 50, Container.HostileTarget.HealthPercent > 80);

            TryCastSpell(Counterspell, 0, 29, Container.HostileTarget.Mana > 0 && Container.HostileTarget.IsCasting);

            TryCastSpell(ManaShield, 0, 50, !Container.Player.HasBuff(ManaShield) && Container.Player.HealthPercent < 20);

            TryCastSpell(FireBlast, 0, 19, !Container.Player.HasBuff(Clearcasting));

            TryCastSpell(FrostNova, 0, 10, !ObjectManager.Instance.Units.Any(u => u.Guid != Container.HostileTarget.Guid && u.Health > 0 && u.Location.GetDistanceTo(Container.Player.Location) < 15), callback: FrostNovaCallback);

            TryCastSpell(Fireball, 0, 34, Container.Player.Level < 15 || Container.Player.HasBuff(PresenceOfMind));

            TryCastSpell(ArcaneMissiles, 0, 29, Container.Player.Level >= 15);
        }

        Action FrostNovaCallback => () =>
        {
            frostNovaBackpedaling = true;
            frostNovaBackpedalStartTime = Environment.TickCount;
            Container.Player.StartMovement(ControlBits.Back);
        };
    }
}
