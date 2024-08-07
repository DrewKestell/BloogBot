using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace WarlockDestruction.Tasks
{
    internal class SummonPetTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            if (ObjectManager.Player.IsCasting)
                return;

            ObjectManager.Player.DoEmote(Emote.EMOTE_STATE_STAND);

            if ((!ObjectManager.Player.IsSpellReady(SummonImp) && !ObjectManager.Player.IsSpellReady(SummonVoidwalker)) || ObjectManager.Pet != null)
            {
                BotTasks.Pop();
                BotTasks.Push(new BuffTask(BotContext));
                return;
            }

            if (ObjectManager.Player.IsSpellReady(SummonImp))
                ObjectManager.Player.CastSpell(SummonImp);
            else
                ObjectManager.Player.CastSpell(SummonVoidwalker);
        }
    }
}
