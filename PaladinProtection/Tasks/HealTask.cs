using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace PaladinProtection.Tasks
{
    internal class HealTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        private const string DivineProtection = "Divine Protection";
        private const string HolyLight = "Holy Light";

        public void Update()
        {
            if (ObjectManager.Player.IsCasting) return;

            if (ObjectManager.Player.HealthPercent > 70 || ObjectManager.Player.Mana < ObjectManager.Player.GetManaCost(HolyLight))
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Mana > ObjectManager.Player.GetManaCost(DivineProtection) && ObjectManager.Player.IsSpellReady(DivineProtection))
                ObjectManager.Player.CastSpell(DivineProtection);

            if (ObjectManager.Player.Mana > ObjectManager.Player.GetManaCost(HolyLight) && ObjectManager.Player.IsSpellReady(HolyLight))
                ObjectManager.Player.CastSpell(HolyLight, 1);
        }
    }
}
