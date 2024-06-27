using WoWActivityMember.Tasks.SharedStates;
using WoWActivityMember.Tasks;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;

namespace WarlockAffliction.Tasks
{
    class PvPRotationTask : CombatRotationTask, IBotTask
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

        internal PvPRotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }


        public void Update()
        {
            if (ObjectManager.Aggressors.Count() == 0)
            {
                BotTasks.Pop();
                return;
            }

            AssignDPSTarget();

            if (ObjectManager.Player.Target == null) return;

            //if (Container.State.TankInPosition)
            //{
            //    if (MoveTowardsTarget())
            //        return;

            //    PerformCombatRotation();
            //}
            else if (MoveBehindTankSpot(15))
                return;
            else
                ObjectManager.Player.StopAllMovement();
        }
        public override void PerformCombatRotation()
        {
            ObjectManager.Player.StopAllMovement();
            ObjectManager.Player.Face(ObjectManager.Player.Target.Position);
            ObjectManager.Pet?.Attack();

            TryCastSpell(LifeTap, 0, int.MaxValue, ObjectManager.Player.HealthPercent > 85 && ObjectManager.Player.ManaPercent < 80);

            // if target is low on health, turn off wand and cast drain soul
            if (ObjectManager.Player.Target.HealthPercent <= 20)
            {
                Functions.LuaCall(TurnOffWandLuaScript);
                TryCastSpell(DrainSoul, 0, 29);
            }
            else
            {
                TryCastSpell(CurseOfAgony, 0, 28, !ObjectManager.Player.Target.HasDebuff(CurseOfAgony) && ObjectManager.Player.Target.HealthPercent > 90);

                TryCastSpell(Immolate, 0, 28, !ObjectManager.Player.Target.HasDebuff(Immolate) && ObjectManager.Player.Target.HealthPercent > 30);

                TryCastSpell(Corruption, 0, 28, !ObjectManager.Player.Target.HasDebuff(Corruption) && ObjectManager.Player.Target.HealthPercent > 30);

                TryCastSpell(SiphonLife, 0, 28, !ObjectManager.Player.Target.HasDebuff(SiphonLife) && ObjectManager.Player.Target.HealthPercent > 50);

                TryCastSpell(ShadowBolt, 0, 28, ObjectManager.Player.Target.HealthPercent > 40);
            }
        }
    }
}
