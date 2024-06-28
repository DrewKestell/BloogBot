using WoWActivityMember.Tasks;
using WoWActivityMember.Tasks.SharedStates;

namespace PriestDiscipline.Tasks
{
    class PvPRotationTask : CombatRotationTask, IBotTask
    {
        const string WandLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Shoot') end";
        const string TurnOffWandLuaScript = "if IsAutoRepeatAction(11) ~= nil then CastSpellByName('Shoot') end";
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
