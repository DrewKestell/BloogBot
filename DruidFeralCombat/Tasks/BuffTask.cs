using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace DruidFeral.Tasks
{
    internal class BuffTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        private const string MarkOfTheWild = "Mark of the Wild";
        private const string Thorns = "Thorns";

        public void Update()
        {
            if ((ObjectManager.Player.HasBuff(MarkOfTheWild) || !ObjectManager.Player.IsSpellReady(MarkOfTheWild)) && (ObjectManager.Player.HasBuff(Thorns) || !ObjectManager.Player.IsSpellReady(Thorns)))
            {
                BotTasks.Pop();
                return;
            }
            
            ObjectManager.Player.CastSpell(MarkOfTheWild);
            ObjectManager.Player.CastSpell(Thorns);
        }
    }
}
