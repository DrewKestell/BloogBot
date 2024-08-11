using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace PriestDiscipline.Tasks
{
    public class PvPRotationTask(IBotContext botContext) : CombatRotationTask(botContext), IBotTask
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
