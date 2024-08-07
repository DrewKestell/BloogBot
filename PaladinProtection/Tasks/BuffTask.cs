using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace PaladinProtection.Tasks
{
    internal class BuffTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {

        public void Update()
        {
            if (!ObjectManager.Player.IsSpellReady(BlessingOfMight) || ObjectManager.Player.HasBuff(BlessingOfMight) || ObjectManager.Player.HasBuff(BlessingOfKings) || ObjectManager.Player.HasBuff(BlessingOfSanctuary))
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsSpellReady(BlessingOfMight) && !ObjectManager.Player.IsSpellReady(BlessingOfKings) && !ObjectManager.Player.IsSpellReady(BlessingOfSanctuary))
                TryCastSpell(BlessingOfMight);

            if (ObjectManager.Player.IsSpellReady(BlessingOfKings) && !ObjectManager.Player.IsSpellReady(BlessingOfSanctuary))
                TryCastSpell(BlessingOfKings);

            if (ObjectManager.Player.IsSpellReady(BlessingOfSanctuary))
                TryCastSpell(BlessingOfSanctuary);
        }

        private void TryCastSpell(string name)
        {
            if (!ObjectManager.Player.HasBuff(name) && ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.Mana > ObjectManager.Player.GetManaCost(name))
            {
                ObjectManager.Player.CastSpell(name, castOnSelf: true);
            }
        }
    }
}
