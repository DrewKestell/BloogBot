using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace HunterBeastMastery.Tasks
{
    internal class BuffTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {

        public void Update()
        {
            if (!ObjectManager.Player.IsSpellReady(AspectOfTheHawk) || ObjectManager.Player.HasBuff(AspectOfTheHawk))
            {
                BotTasks.Pop();
                return;
            }

            ObjectManager.Player.CastSpell(AspectOfTheHawk);
        }
    }
}
