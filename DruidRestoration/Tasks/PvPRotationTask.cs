using WoWActivityMember.Tasks.SharedStates;
using WoWActivityMember.Tasks;

namespace DruidRestoration.Tasks
{
    class PvPRotationTask : CombatRotationTask, IBotTask
    {
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
