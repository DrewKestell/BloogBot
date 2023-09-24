using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using BloogBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.AI.SharedStates
{
    public class TurnInQuestFromNpcState : BotState, IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;
        readonly int startTime = Environment.TickCount;
        readonly string questName;
        readonly int rewardSelection;

        State state = State.Uninitialized;
        WoWUnit npc;

        public TurnInQuestFromNpcState(Stack<IBotState> botStates, IDependencyContainer container, string npcName, QuestTask questTask)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;

            npc = ObjectManager.Units
                .Where(x => x.Name == npcName)
                .First();

            this.questName = questTask.Name;
            this.rewardSelection = InventoryHelper.GetBestQuestReward(questTask);
        }

        public void Update()
        {
            if (player.IsInCombat || (Environment.TickCount - startTime > 15000))
            {
                botStates.Pop();
                return;
            }

            SetCurrentState();

            if (state == State.Uninitialized)
            {
                player.StopAllMovement();
                npc.Interact();
            }
            if (state == State.GossipQuest)
            {
                for (int i = 1; i < 5; i++)
                {
                    string questGossipText = FrameHelper.GetQuestGossipOption(i);
                    string questTitleText = FrameHelper.GetQuestTitleOption(i);

                    if (questGossipText.Equals(questName, StringComparison.Ordinal) || questTitleText.Equals(questName, StringComparison.Ordinal))
                    {
                        Functions.LuaCall($"GossipTitleButton{i}:Click()");
                        Functions.LuaCall($"QuestTitleButton{i}:Click()");
                        break;
                    }

                    if (i == 5)
                    {
                        state = State.ReadyToPop;
                    }
                }
            }
            if (state == State.SelectReward)
            {
                Functions.LuaCall($"QuestRewardItem{rewardSelection}:Click()");
                state = State.AcceptReward;
            }
            if (state == State.AcceptReward)
            {
                Functions.LuaCall("QuestFrameCompleteButton:Click()");
                Functions.LuaCall("QuestFrameCompleteQuestButton:Click()");
            }
            if (state == State.ReadyToPop)
            {
                Functions.LuaCall("QuestFrameCloseButton:Click()");
                botStates.Pop();
            }

            Wait.For("QuestFrameAnim", 300);
        }
        public void SetCurrentState()
        {
            if (FrameHelper.IsElementVisibile("GossipFrame"))
            {
                state = State.GossipQuest;
            }
            else if (FrameHelper.IsElementVisibile($"QuestRewardItem{rewardSelection}"))
            {
                state = State.SelectReward;
            }
            else if (FrameHelper.IsElementVisibile("QuestFrameCompleteQuestButton") || FrameHelper.IsElementVisibile("QuestFrameCompleteButton"))
            {
                state = State.AcceptReward;
            }
            else if ((npc.NpcMarkerFlags & NpcMarkerFlags.YellowQuestion) != NpcMarkerFlags.YellowQuestion || (FrameHelper.IsElementVisibile("QuestFrame") && (state != State.Uninitialized || FrameHelper.IsElementVisibile("QuestFrameAcceptButton"))))
            {
                state = State.ReadyToPop;
            }
        }

        enum State
        {
            Uninitialized,
            GossipQuest,
            SelectReward,
            AcceptReward,
            ReadyToPop
        }
    }
}
