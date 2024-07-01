using WoWActivityMember.Tasks;
using WoWActivityMember.Tasks.SharedStates;

namespace MageFire.Tasks
{
    internal class PvPRotationTask : CombatRotationTask, IBotTask
    {
        private const string WandLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Shoot') end";
        private const string TurnOffWandLuaScript = "if IsAutoRepeatAction(11) ~= nil then CastSpellByName('Shoot') end";

        internal PvPRotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        public void Update()
        {
            BotTasks.Pop();
        }
        public override void PerformCombatRotation()
        {

        }
    }
}
