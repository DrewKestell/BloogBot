using WoWActivityMember.Game.Frames;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Objects;

namespace WoWActivityMember.Tasks.SharedStates
{
    public class BuyItemsTask(IClassContainer container, Stack<IBotTask> botTasks, string npcName, IDictionary<string, int> itemsToBuy) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private readonly string npcName = npcName;
        private readonly LocalPlayer player = ObjectManager.Player;
        private readonly IDictionary<string, int> itemsToBuy = itemsToBuy;
        private readonly WoWUnit npc;
        private readonly DialogFrame dialogFrame;
        private readonly MerchantFrame merchantFrame;

        public void Update()
        {
        }
    }
}