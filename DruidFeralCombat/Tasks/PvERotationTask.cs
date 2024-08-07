using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;
using static BotRunner.Constants.Spellbook;

namespace DruidFeral.Tasks
{
    internal class PvERotationTask(IBotContext botContext) : CombatRotationTask(botContext), IBotTask
    {
        private const string AutoAttackLuaScript = "if IsCurrentAction('12') == nil then CastSpellByName('Attack') end";

        private Position targetLastPosition;

        public void Update()
        {
            if (!ObjectManager.Aggressors.Any())
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Target == null || ObjectManager.Player.Target.HealthPercent <= 0)
            {
                ObjectManager.Player.SetTarget(ObjectManager.Aggressors.First().Guid);
            }

            if (Update(3))
                return;

            if (ObjectManager.Player.HealthPercent < 30 && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(HealingTouch))
            {
                if (ObjectManager.Player.CurrentShapeshiftForm == BearForm && Wait.For("BearFormDelay", 1000, true))
                    CastSpell(BearForm);

                if (ObjectManager.Player.CurrentShapeshiftForm == CatForm && Wait.For("CatFormDelay", 1000, true))
                    CastSpell(CatForm);

                Wait.RemoveAll();
                BotTasks.Push(new HealTask(BotContext));
                return;
            }

            if (ObjectManager.Player.Target.TappedByOther)
            {
                ObjectManager.Player.StopAllMovement();
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Target.Health == 0)
            {
                const string waitKey = "PopCombatState";

                if (Wait.For(waitKey, 1500))
                {
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                    BotTasks.Push(new LootTask(BotContext));
                    Wait.Remove(waitKey);
                }

                return;
            }

            if (ObjectManager.Player.TargetGuid == ObjectManager.Player.Guid)
                ObjectManager.Player.SetTarget(ObjectManager.Player.TargetGuid);

            // ensure we're facing the ObjectManager.Player.Target
            if (!ObjectManager.Player.IsFacing(ObjectManager.Player.Target.Position)) ObjectManager.Player.Face(ObjectManager.Player.Target.Position);

            // ensure auto-attack is turned on
            ObjectManager.Player.StartMeleeAttack();

            // if less than level 13, use spellcasting
            if (ObjectManager.Player.Level <= 12)
            {
                // if low on mana, move into melee range
                if (ObjectManager.Player.ManaPercent < 20 && ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) > 5)
                {
                    ObjectManager.Player.MoveToward(ObjectManager.Player.Target.Position);
                    return;
                }
                else ObjectManager.Player.StopAllMovement();

                TryCastSpell(Moonfire, 0, 10, !ObjectManager.Player.Target.HasDebuff(Moonfire));

                TryCastSpell(Wrath, 10, 30);
            }
            // bear form
            else if (ObjectManager.Player.Level > 12 && ObjectManager.Player.Level < 20)
            {
                // ensure we're in melee range
                if ((ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) > 3 && ObjectManager.Player.CurrentShapeshiftForm == BearForm && ObjectManager.Player.Target.IsInCombat && !TargetMovingTowardPlayer) || (!ObjectManager.Player.Target.IsInCombat && ObjectManager.Player.IsCasting))
                {
                    Position[] nextWaypoint = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);
                    ObjectManager.Player.MoveToward(nextWaypoint[0]);
                }
                else
                    ObjectManager.Player.StopAllMovement();

                TryCastSpell(BearForm, 0, 50, ObjectManager.Player.CurrentShapeshiftForm != BearForm && Wait.For("BearFormDelay", 1000, true));

                if (ObjectManager.Aggressors.Count() > 1)
                {
                    TryUseBearAbility(DemoralizingRoar, 10, !ObjectManager.Player.Target.HasDebuff(DemoralizingRoar) && ObjectManager.Player.CurrentShapeshiftForm == BearForm);
                }

                TryUseBearAbility(Enrage, condition: ObjectManager.Player.CurrentShapeshiftForm == BearForm);

                TryUseBearAbility(Maul, (int)Math.Max(15 - (ObjectManager.Player.Level - 9), 10), ObjectManager.Player.CurrentShapeshiftForm == BearForm);
            }
            // cat form
            else if (ObjectManager.Player.Level >= 20)
            {
                // ensure we're in melee range
                if ((ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) > 3 && ObjectManager.Player.CurrentShapeshiftForm == CatForm && ObjectManager.Player.Target.IsInCombat && !TargetMovingTowardPlayer) || (!ObjectManager.Player.Target.IsInCombat && ObjectManager.Player.IsCasting))
                {
                    Position[] nextWaypoint = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);
                    ObjectManager.Player.MoveToward(nextWaypoint[0]);
                }
                else
                    ObjectManager.Player.StopAllMovement();

                TryCastSpell(CatForm, 0, 50, ObjectManager.Player.CurrentShapeshiftForm != CatForm);

                TryUseCatAbility(TigersFury, 30, condition: ObjectManager.Player.Target.HealthPercent > 30 && !ObjectManager.Player.HasBuff(TigersFury));

                TryUseCatAbility(Rake, 35, condition: ObjectManager.Player.Target.HealthPercent > 50 && !ObjectManager.Player.Target.HasDebuff(Rake));

                TryUseCatAbility(Claw, 40);

                //TryUseCatAbility(Rip, 30, true, (ObjectManager.Player.Target.HealthPercent < 70 && !ObjectManager.Player.Target.HasDebuff(Rip)));
            }

            targetLastPosition = ObjectManager.Player.Target.Position;
        }

        private void TryUseBearAbility(string name, int requiredRage = 0, bool condition = true, Action callback = null)
        {
            if (ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.Rage >= requiredRage && !ObjectManager.Player.IsStunned && ObjectManager.Player.CurrentShapeshiftForm == BearForm && condition)
            {
                ObjectManager.Player.CastSpell(name);
                callback?.Invoke();
            }
        }

        private void TryUseCatAbility(string name, int requiredEnergy = 0, bool requiresComboPoints = false, bool condition = true, Action callback = null)
        {
            if (ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.Energy >= requiredEnergy && (!requiresComboPoints || ObjectManager.Player.ComboPoints > 0) && !ObjectManager.Player.IsStunned && ObjectManager.Player.CurrentShapeshiftForm == CatForm && condition)
            {
                ObjectManager.Player.CastSpell(name);
                callback?.Invoke();
            }
        }

        private void CastSpell(string name)
        {
            if (ObjectManager.Player.IsSpellReady(name) && !ObjectManager.Player.IsCasting)
                ObjectManager.Player.CastSpell(name);
        }

        private void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null)
        {
            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position);

            if (ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.Mana >= ObjectManager.Player.GetManaCost(name) && distanceToTarget >= minRange && distanceToTarget <= maxRange && condition && !ObjectManager.Player.IsStunned && ObjectManager.Player.IsCasting && ObjectManager.Player.ChannelingId == 0)
            {
                ObjectManager.Player.CastSpell(name);
                callback?.Invoke();
            }
        }

        public override void PerformCombatRotation()
        {

        }
    }
}
