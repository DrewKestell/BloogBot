using WoWActivityMember.Tasks;
using WoWActivityMember.Tasks.SharedStates;

namespace PriestDiscipline.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {
        private const string WandLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Shoot') end";
        private const string TurnOffWandLuaScript = "if IsAutoRepeatAction(11) ~= nil then CastSpellByName('Shoot') end";

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }


        public void Update()
        {
            BotTasks.Pop();
        }
        public override void PerformCombatRotation()
        {

        }
    }
}
