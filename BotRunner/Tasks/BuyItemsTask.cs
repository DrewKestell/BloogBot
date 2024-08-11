using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    public class BuyItemsTask(IBotContext botContext, string npcName, IDictionary<string, int> itemsToBuy) : BotTask(botContext), IBotTask
    {
        private readonly string npcName = npcName;
        private readonly IDictionary<string, int> itemsToBuy = itemsToBuy;
        private readonly IWoWUnit npc;
        private readonly IDialogFrame dialogFrame;
        private readonly IMerchantFrame merchantFrame;

        public void Update()
        {
        }
    }
}