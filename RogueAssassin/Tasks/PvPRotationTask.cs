using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace RogueAssassin.Tasks
{
    internal class PvPRotationTask : CombatRotationTask, IBotTask
    {
        internal PvPRotationTask(IBotContext botContext) : base(botContext) { }

        public void Update()
        {
            BotTasks.Pop();
        }
        public override void PerformCombatRotation()
        {

        }
    }
}
