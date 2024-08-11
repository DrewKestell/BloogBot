using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace PriestShadow.Tasks
{
    public class PvPRotationTask(IBotContext botContext) : CombatRotationTask(botContext), IBotTask
    {
        public void Update()
        {

        }
        public override void PerformCombatRotation()
        {
            
        }
    }
}
