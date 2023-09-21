using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BloogBot.AI.SharedStates
{
    public class PickUpQuestFromNpcState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly string npcName;
        readonly LocalPlayer player;
        readonly int startTime = Environment.TickCount;
        readonly int currentQuestLogSize;

        State state = State.Uninitialized;
        WoWUnit npc;

        public PickUpQuestFromNpcState(Stack<IBotState> botStates, IDependencyContainer container, string npcName)
        {
            this.botStates = botStates;
            this.container = container;
            this.npcName = npcName;
            player = ObjectManager.Player;

            npc = ObjectManager.Units
                .Where(x => x.Name == npcName)
                .First();

            currentQuestLogSize = QuestHelper.GetQuestsFromQuestLog().Count;
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
                ObjectManager.Player.StopAllMovement();
                npc.Interact();
            }
            if (state == State.GossipQuest)
            {
                if (IsElementVisibile($"GossipTitleButton1"))
                {
                    Functions.LuaCall($"GossipTitleButton1:Click()");
                }
                else if (IsElementVisibile($"QuestTitleButton1"))
                {
                    Functions.LuaCall($"QuestTitleButton1:Click()");
                }
            }
            if (state == State.AcceptQuest)
            {
                Functions.LuaCall("QuestFrameAcceptButton:Click()");
            }
            if (state == State.CloseQuest)
            {
                Functions.LuaCall("QuestFrameCloseButton:Click()");
            }

            if (state == State.ReadyToPop)
            {
                botStates.Pop();
            }
        }
        public void SetCurrentState()
        {
            if (npc.NpcMarkerFlags != NpcMarkerFlags.YellowExclamation)
            {
                state = State.ReadyToPop;
            }
            else if (IsElementVisibile("QuestFrameAcceptButton"))
            {
                state = State.AcceptQuest;
            }
            else if (IsElementVisibile("GossipFrame"))
            {
                state = State.GossipQuest;
            }
            else if (IsElementVisibile("QuestFrameCloseButton"))
            {
                state = State.CloseQuest;
            }
        }

        enum State
        {
            Uninitialized,
            GossipQuest,
            AcceptQuest,
            CloseQuest,
            ReadyToPop
        }

        public static string GetQuestGossipOption(int i)
        {
            var hasOption = Functions.LuaCallWithResult($"{{0}} = GossipTitleButton{i}:IsVisible()");
            if (hasOption.Length > 0 && hasOption[0] == "1")
            {
                string[] results = Functions.LuaCallWithResult($"{{0}} = GossipTitleButton{i}:GetText()");
                if (results.Length > 0)
                {
                    return results[0];
                }
            }
            return string.Empty;
        }

        public static string GetQuestTitleOption(int i)
        {
            var hasOption = Functions.LuaCallWithResult($"{{0}} = QuestTitleButton{i}:IsVisible()");
            if (hasOption.Length > 0 && hasOption[0] == "1")
            {
                string[] results = Functions.LuaCallWithResult($"{{0}} = QuestTitleButton{i}:GetText()");
                if (results.Length > 0)
                {
                    return results[0];
                }
            }
            return string.Empty;
        }
        public static bool IsElementVisibile(string elementName)
        {
            var hasOption = Functions.LuaCallWithResult($"{{0}} = {elementName}:IsVisible()");

            return hasOption.Length > 0 && hasOption[0] == "1";
        }
    }
}
