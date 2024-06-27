using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Tasks;
using WoWActivityMember.Tasks.SharedStates;
using static WoWActivityMember.Constants.Enums;

namespace MageArcane.Tasks
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

        bool frostNovaBackpedaling;
        int frostNovaBackpedalStartTime;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        public void Update()
        {
            if (frostNovaBackpedaling && Environment.TickCount - frostNovaBackpedalStartTime > 1500)
            {
                ObjectManager.Player.StopMovement(ControlBits.Back);
                frostNovaBackpedaling = false;
            }
            if (frostNovaBackpedaling)
                return;


            if (ObjectManager.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Target == null || ObjectManager.Player.Target.HealthPercent <= 0)
            {
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors[0].Guid);
            }

            if (Update(30))
                return;

            bool hasWand = Inventory.GetEquippedItem(EquipSlot.Ranged) != null;
            bool useWand = hasWand && ObjectManager.Player.ManaPercent <= 10 && ObjectManager.Player.IsCasting && ObjectManager.Player.ChannelingId == 0;
            if (useWand)
                Functions.LuaCall(WandLuaScript);

            TryCastSpell(PresenceOfMind, 0, 50, ObjectManager.Player.Target.HealthPercent > 80);

            TryCastSpell(ArcanePower, 0, 50, ObjectManager.Player.Target.HealthPercent > 80);

            TryCastSpell(Counterspell, 0, 29, ObjectManager.Player.Target.Mana > 0 && ObjectManager.Player.Target.IsCasting);

            TryCastSpell(ManaShield, 0, 50, !ObjectManager.Player.HasBuff(ManaShield) && ObjectManager.Player.HealthPercent < 20);

            TryCastSpell(FireBlast, 0, 19, !ObjectManager.Player.HasBuff(Clearcasting));

            TryCastSpell(FrostNova, 0, 10, !ObjectManager.Units.Any(u => u.Guid != ObjectManager.Player.TargetGuid && u.Health > 0 && u.Position.DistanceTo(ObjectManager.Player.Position) < 15), callback: FrostNovaCallback);

            TryCastSpell(Fireball, 0, 34, ObjectManager.Player.Level < 15 || ObjectManager.Player.HasBuff(PresenceOfMind));

            TryCastSpell(ArcaneMissiles, 0, 29, ObjectManager.Player.Level >= 15);
        }

        public override void PerformCombatRotation()
        {
            throw new NotImplementedException();
        }

        Action FrostNovaCallback => () =>
        {
            frostNovaBackpedaling = true;
            frostNovaBackpedalStartTime = Environment.TickCount;
            ObjectManager.Player.StartMovement(ControlBits.Back);
        };
    }
}
