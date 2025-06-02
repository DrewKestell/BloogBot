using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace ShamanElemental.Tasks
{
    internal class HealTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            if (ObjectManager.Player.IsCasting) return;

            if (ObjectManager.Player.HealthPercent > 70 || ObjectManager.Player.Mana < ObjectManager.Player.GetManaCost(HealingWave))
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsSpellReady(WarStomp))
                ObjectManager.Player.CastSpell(WarStomp);

            ObjectManager.Player.CastSpell(HealingWave, castOnSelf: true);
        }
    }
}
