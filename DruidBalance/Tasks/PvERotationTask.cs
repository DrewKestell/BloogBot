using WoWActivityMember.Tasks;
using WoWActivityMember.Tasks.SharedStates;
using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Objects;
using static WoWActivityMember.Constants.Enums;

namespace DruidBalance.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {
        private static readonly string[] ImmuneToNatureDamage = { "Vortex", "Whirlwind", "Whirling", "Dust", "Cyclone" };
        private const string AbolishPoison = "Abolish Poison";
        private const string EntanglingRoots = "Entangling Roots";
        private const string HealingTouch = "Healing Touch";
        private const string Moonfire = "Moonfire";
        private const string Rejuvenation = "Rejuvenation";
        private const string RemoveCurse = "Remove Curse";
        private const string Wrath = "Wrath";
        private const string InsectSwarm = "Insect Swarm";
        private const string Innervate = "Innervate";
        private const string MoonkinForm = "Moonkin Form";
        private WoWUnit secondaryTarget;
        private bool castingEntanglingRoots;
        private bool backpedaling;
        private int backpedalStartTime;

        private Action EntanglingRootsCallback => () =>
        {
            castingEntanglingRoots = true;
        };

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        public void Update()
        {
            if (castingEntanglingRoots)
            {
                if (secondaryTarget.HasDebuff(EntanglingRoots))
                {
                    backpedaling = true;
                    backpedalStartTime = Environment.TickCount;
                    ObjectManager.Player.StartMovement(ControlBits.Back);
                }

                ObjectManager.Player.SetTarget(ObjectManager.Player.TargetGuid);
                castingEntanglingRoots = false;
            }

            // handle backpedaling during entangling roots
            if (Environment.TickCount - backpedalStartTime > 1500)
            {
                ObjectManager.Player.StopMovement(ControlBits.Back);
                backpedaling = false;
            }
            if (backpedaling)
                return;

            // heal self if we're injured
            if (ObjectManager.Player.HealthPercent < 30 && (ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(HealingTouch) || ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(Rejuvenation)))
            {
                Wait.RemoveAll();
                BotTasks.Push(new HealTask(Container, BotTasks));
                return;
            }

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

            // if we get an add, root it with Entangling Roots
            if (ObjectManager.Aggressors.Count() == 2 && secondaryTarget == null)
                secondaryTarget = ObjectManager.Aggressors.Single(u => u.Guid != ObjectManager.Player.TargetGuid);

            if (secondaryTarget != null && !secondaryTarget.HasDebuff(EntanglingRoots))
            {
                ObjectManager.Player.SetTarget(secondaryTarget.Guid);
                TryCastSpell(EntanglingRoots, 0, 30, !secondaryTarget.HasDebuff(EntanglingRoots), EntanglingRootsCallback);
            }

            TryCastSpell(MoonkinForm, !ObjectManager.Player.HasBuff(MoonkinForm));

            TryCastSpell(Innervate, ObjectManager.Player.ManaPercent < 10, castOnSelf: true);

            TryCastSpell(RemoveCurse, 0, int.MaxValue, ObjectManager.Player.IsCursed && !ObjectManager.Player.HasBuff(MoonkinForm), castOnSelf: true);

            TryCastSpell(AbolishPoison, 0, int.MaxValue, ObjectManager.Player.IsPoisoned && !ObjectManager.Player.HasBuff(MoonkinForm), castOnSelf: true);

            TryCastSpell(InsectSwarm, 0, 30, !ObjectManager.Player.Target.HasDebuff(InsectSwarm) && ObjectManager.Player.Target.HealthPercent > 20 && !ImmuneToNatureDamage.Any(s => ObjectManager.Player.Target.Name.Contains(s)));

            TryCastSpell(Moonfire, 0, 30, !ObjectManager.Player.Target.HasDebuff(Moonfire));

            TryCastSpell(Wrath, 0, 30, !ImmuneToNatureDamage.Any(s => ObjectManager.Player.Target.Name.Contains(s)));
        }

        public override void PerformCombatRotation()
        {
            throw new NotImplementedException();
        }
    }
}
