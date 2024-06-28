using WoWActivityMember.Tasks;
using WoWActivityMember.Tasks.SharedStates;

namespace ShamanRestoration.Tasks
{
    class PvERotationTask : CombatRotationTask, IBotTask
    {
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
