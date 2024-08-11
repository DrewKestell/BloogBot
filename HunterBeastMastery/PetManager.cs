using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using HunterBeastMastery.Tasks;
using static BotRunner.Constants.Spellbook;

namespace HunterBeastMastery
{
    internal class PetManagerState(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            if (ObjectManager.Player.IsCasting)
                return;

            if (!ObjectManager.Player.IsSpellReady(CallPet) || ObjectManager.Pet != null)
            {
                ObjectManager.Player.DoEmote(Emote.EMOTE_STATE_STAND);
                BotTasks.Pop();
                BotTasks.Push(new BuffTask(BotContext));
                return;
            }

            ObjectManager.Player.CastSpell(CallPet);
        }
    }
}
