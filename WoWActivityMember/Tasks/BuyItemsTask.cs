using WoWActivityMember.Game.Frames;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Objects;

namespace WoWActivityMember.Tasks.SharedStates
{
    public class BuyItemsTask(IClassContainer container, Stack<IBotTask> botTasks, string npcName, IDictionary<string, int> itemsToBuy) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        readonly string npcName = npcName;
        readonly LocalPlayer player = ObjectManager.Player;
        readonly IDictionary<string, int> itemsToBuy = itemsToBuy;

        readonly WoWUnit npc;
        readonly DialogFrame dialogFrame;
        readonly MerchantFrame merchantFrame;

        public void Update()
        {
        }
    }
}