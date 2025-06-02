using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace MageArcane.Tasks
{
    internal class PvPRotationTask(IBotContext botContext) : CombatRotationTask(botContext), IBotTask
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
