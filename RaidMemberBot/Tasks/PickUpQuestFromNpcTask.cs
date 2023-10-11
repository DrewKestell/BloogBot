using RaidMemberBot.Game.Frames;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.AI.SharedStates
{
    public class PickUpQuestFromNpcTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly string npcName;
        readonly LocalPlayer player;
        readonly int startTime = Environment.TickCount;
        readonly int currentQuestLogSize;

        WoWUnit npc;
        GossipFrame dialogFrame;

        public PickUpQuestFromNpcTask(IClassContainer container, Stack<IBotTask> botTasks, string npcName)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.npcName = npcName;
            player = ObjectManager.Instance.Player;

            npc = ObjectManager.Instance
                .Units
                .Where(x => x.Name == npcName)
                .First();
        }

        public void Update()
        {
            if (player.IsInCombat || (Environment.TickCount - startTime > 5000))
            {
                botTasks.Pop();
                return;
            }
        }
    }
}
