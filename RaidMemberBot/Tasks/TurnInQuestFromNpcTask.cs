using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.AI.SharedStates
{
    public class TurnInQuestFromNpcTask : BotTask, IBotTask
    {
        readonly LocalPlayer player;
        readonly int startTime = Environment.TickCount;
        readonly string questName;
        readonly int rewardSelection;

        WoWUnit npc;

        public TurnInQuestFromNpcTask(IClassContainer container, Stack<IBotTask> botTasks, string npcName, string questName) : base(container, botTasks, TaskType.Ordinary)
        {
            player = ObjectManager.Instance.Player;

            npc = ObjectManager.Instance
                .Units
.First(x => x.Name == npcName);

            this.questName = questName;
        }

        public void Update()
        {
            if (Container.Player.IsInCombat || (Environment.TickCount - startTime > 5000))
            {
                BotTasks.Pop();
                return;
            }

        }
    }
}
