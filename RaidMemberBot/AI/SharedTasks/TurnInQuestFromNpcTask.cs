using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.AI.SharedStates
{
    public class TurnInQuestFromNpcTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;
        readonly int startTime = Environment.TickCount;
        readonly string questName;
        readonly int rewardSelection;

        WoWUnit npc;

        public TurnInQuestFromNpcTask(IClassContainer container, Stack<IBotTask> botTasks, string npcName, string questName)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;

            npc = ObjectManager.Instance
                .Units
                .Where(x => x.Name == npcName)
                .First();

            this.questName = questName;
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
