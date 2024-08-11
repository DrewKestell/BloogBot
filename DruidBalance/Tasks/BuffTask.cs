using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace DruidBalance.Tasks
{
    internal class BuffTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            if ((ObjectManager.Player.HasBuff(MarkOfTheWild) || !ObjectManager.Player.IsSpellReady(MarkOfTheWild)) &&
                (ObjectManager.Player.HasBuff(Thorns) || !ObjectManager.Player.IsSpellReady(Thorns)) &&
                (ObjectManager.Player.HasBuff(OmenOfClarity) || !ObjectManager.Player.IsSpellReady(OmenOfClarity)))
            {
                BotTasks.Pop();
                return;
            }

            if (!ObjectManager.Player.HasBuff(MarkOfTheWild))
            {
                if (ObjectManager.Player.HasBuff(MoonkinForm))
                {
                    ObjectManager.Player.CastSpell(MoonkinForm);
                }

                ObjectManager.Player.CastSpell(MarkOfTheWild);
            }

            ObjectManager.Player.CastSpell(Thorns);
            ObjectManager.Player.CastSpell(OmenOfClarity);
        }
    }
}
