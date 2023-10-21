using RaidMemberBot.Game.Frames;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
{
    public class BuyItemsTask : BotTask, IBotTask
    {
        readonly string npcName;
        readonly LocalPlayer player;
        readonly IDictionary<string, int> itemsToBuy;

        WoWUnit npc;
        GossipFrame dialogFrame;
        MerchantFrame merchantFrame;
        
        public BuyItemsTask(IClassContainer container, Stack<IBotTask> botTasks, string npcName, IDictionary<string, int> itemsToBuy) : base(container, botTasks, TaskType.Ordinary)
        {
            this.npcName = npcName;
            this.itemsToBuy = itemsToBuy;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
        }
    }
}