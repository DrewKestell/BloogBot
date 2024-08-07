using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace MageArcane.Tasks
{
    internal class BuffTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            if ((!ObjectManager.Player.IsSpellReady(ArcaneIntellect) || ObjectManager.Player.HasBuff(ArcaneIntellect)) && (ObjectManager.Player.HasBuff(FrostArmor) || ObjectManager.Player.HasBuff(IceArmor)) && (!ObjectManager.Player.IsSpellReady(DampenMagic) || ObjectManager.Player.HasBuff(DampenMagic)))
            {
                BotTasks.Pop();
                BotTasks.Push(new ConjureItemsTask(BotContext));
                return;
            }
            
            TryCastSpell(ArcaneIntellect, castOnSelf: true);

            if (ObjectManager.Player.IsSpellReady(IceArmor))
                TryCastSpell(IceArmor);
            else
                TryCastSpell(FrostArmor);

            TryCastSpell(DampenMagic, castOnSelf: true);
        }

        private void TryCastSpell(string name, bool castOnSelf = false)
        {
            if (!ObjectManager.Player.HasBuff(name) && ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.IsSpellReady(name))
            {
                ObjectManager.Player.CastSpell(name,castOnSelf: castOnSelf);
            }
        }
    }
}
