using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace DruidBalance.Tasks
{
    internal class HealTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {

        public void Update()
        {
            if (ObjectManager.Player.IsCasting) return;

            if (ObjectManager.Player.HealthPercent > 70 || (ObjectManager.Player.Mana < ObjectManager.Player.GetManaCost(HealingTouch) && ObjectManager.Player.Mana < ObjectManager.Player.GetManaCost(Rejuvenation)))
            {
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsSpellReady(WarStomp) && ObjectManager.Player.Position.DistanceTo(ObjectManager.GetTarget(ObjectManager.Player).Position) <= 8)
                ObjectManager.Player.CastSpell(WarStomp);

            TryCastSpell(MoonkinForm, ObjectManager.Player.HasBuff(MoonkinForm));

            TryCastSpell(Barkskin);

            TryCastSpell(Rejuvenation, !ObjectManager.Player.HasBuff(Rejuvenation));

            TryCastSpell(HealingTouch);
        }

        private void TryCastSpell(string name, bool condition = true)
        {
            if (ObjectManager.Player.IsSpellReady(name) && condition)
                ObjectManager.Player.CastSpell(name);
        }
    }
}
