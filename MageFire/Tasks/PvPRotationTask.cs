using WoWActivityMember.Game.Statics;
using WoWActivityMember.Tasks;
using WoWActivityMember.Tasks.SharedStates;

namespace MageFire.Tasks
{
    class PvPRotationTask : CombatRotationTask, IBotTask
    {
        const string WandLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Shoot') end";
        const string TurnOffWandLuaScript = "if IsAutoRepeatAction(11) ~= nil then CastSpellByName('Shoot') end";

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
            //else if (MoveBehindTankSpot(15))
            //    return;
            //else
            //    ObjectManager.Player.StopAllMovement();
        }
        public override void PerformCombatRotation()
        {

        }
    }
}
