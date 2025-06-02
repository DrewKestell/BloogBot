using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace MageArcane.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {
        private bool frostNovaBackpedaling;
        private int frostNovaBackpedalStartTime;

        internal PvERotationTask(IBotContext botContext) : base(botContext) { }

        public void Update()
        {
            if (frostNovaBackpedaling && Environment.TickCount - frostNovaBackpedalStartTime > 1500)
            {
                ObjectManager.Player.StopMovement(ControlBits.Back);
                frostNovaBackpedaling = false;
            }
            if (frostNovaBackpedaling)
                return;


            if (!ObjectManager.Aggressors.Any())
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.GetTarget(ObjectManager.Player) == null || ObjectManager.GetTarget(ObjectManager.Player).HealthPercent <= 0)
            {
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors.First().Guid);
            }

            if (Update(30))
                return;

            bool hasWand = ObjectManager.GetEquippedItem(EquipSlot.Ranged) != null;
            bool useWand = hasWand && ObjectManager.Player.ManaPercent <= 10 && ObjectManager.Player.IsCasting && ObjectManager.Player.ChannelingId == 0;
            if (useWand)
                ObjectManager.Player.StartWand();

            TryCastSpell(PresenceOfMind, 0, 50, ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 80);

            TryCastSpell(ArcanePower, 0, 50, ObjectManager.GetTarget(ObjectManager.Player).HealthPercent > 80);

            TryCastSpell(Counterspell, 0, 29, ObjectManager.GetTarget(ObjectManager.Player).Mana > 0 && ObjectManager.GetTarget(ObjectManager.Player).IsCasting);

            TryCastSpell(ManaShield, 0, 50, !ObjectManager.Player.HasBuff(ManaShield) && ObjectManager.Player.HealthPercent < 20);

            TryCastSpell(FireBlast, 0, 19, !ObjectManager.Player.HasBuff(Clearcasting));

            TryCastSpell(FrostNova, 0, 10, !ObjectManager.Units.Any(u => u.Guid != ObjectManager.GetTarget(ObjectManager.Player).Guid && u.Health > 0 && u.Position.DistanceTo(ObjectManager.Player.Position) < 15), callback: FrostNovaCallback);

            TryCastSpell(Fireball, 0, 34, ObjectManager.Player.Level < 15 || ObjectManager.Player.HasBuff(PresenceOfMind));

            TryCastSpell(ArcaneMissiles, 0, 29, ObjectManager.Player.Level >= 15);
        }

        public override void PerformCombatRotation()
        {
            throw new NotImplementedException();
        }

        private Action FrostNovaCallback => () =>
        {
            frostNovaBackpedaling = true;
            frostNovaBackpedalStartTime = Environment.TickCount;
            ObjectManager.Player.StartMovement(ControlBits.Back);
        };
    }
}
