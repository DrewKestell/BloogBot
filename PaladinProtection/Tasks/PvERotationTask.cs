using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace PaladinProtection.Tasks
{
    internal class PvERotationTask : CombatRotationTask, IBotTask
    {
        internal PvERotationTask(IBotContext botContext) : base(botContext) { }

        public override void PerformCombatRotation()
        {

        }

        public void Update()
        {
            BotTasks.Pop();
        }
    }
}
