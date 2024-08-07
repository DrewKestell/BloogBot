using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace MageFire.Tasks
{
    internal class PvERotationTask(IBotContext botContext) : CombatRotationTask(botContext), IBotTask
    {
        public void Update()
        {
            BotTasks.Pop();
        }
        public override void PerformCombatRotation()
        {

        }
    }
}
