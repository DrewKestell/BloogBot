

using BloogBot.Game;
using System;
using System.Threading;

namespace BloogBot.AI
{
    public static class DialogueHelper
    {
        public static void PickupQuestFromNpc()
        {
            if (IsElementVisibile("QuestFrameAcceptButton"))
            {
                ObjectManager.Player.LuaCall("QuestFrameAcceptButton:Click()");

                Thread.Sleep(500);
            }

            for (int i = 20; i > 0; i--)
            {
                if (IsElementVisibile($"GossipTitleButton{i}"))
                {
                    ObjectManager.Player.LuaCall($"GossipTitleButton{i}:Click()");

                    Thread.Sleep(500);
                }

                if (IsElementVisibile($"QuestTitleButton{i}"))
                {
                    ObjectManager.Player.LuaCall($"QuestTitleButton{i}:Click()");

                    Thread.Sleep(500);
                }
            }

            if (IsElementVisibile("QuestFrameAcceptButton"))
            {
                ObjectManager.Player.LuaCall("QuestFrameAcceptButton:Click()");

                Thread.Sleep(500);
            }

            if (IsElementVisibile("QuestFrameCloseButton"))
            {
                ObjectManager.Player.LuaCall("QuestFrameCloseButton:Click()");

                Thread.Sleep(500);
            }
        }
        public static void TurnInQuestFromNpc(string questName, int questReward = 1)
        {
            for (int i = 1; i < 5; i++)
            {
                string questGossipText = GetQuestGossipOption(i);
                string questTitleText = GetQuestTitleOption(i);

                if (questGossipText.Equals(questName, StringComparison.Ordinal) || questTitleText.Equals(questName, StringComparison.Ordinal))
                {
                    ObjectManager.Player.LuaCall($"GossipTitleButton{i}:Click()");
                    ObjectManager.Player.LuaCall($"QuestTitleButton{i}:Click()");
                    break;
                }
            }

            Thread.Sleep(300);

            CompleteOpenQuestFrame(questReward);
        }

        public static void CompleteOpenQuestFrame(int questReward)
        {
            if (IsElementVisibile("QuestFrameCompleteButton"))
            {
                ObjectManager.Player.LuaCall($@"QuestFrameCompleteButton:Click()");

                Thread.Sleep(300);
            }

            if (IsElementVisibile($"QuestRewardItem{questReward}"))
            {
                ObjectManager.Player.LuaCall($"QuestRewardItem{questReward}:Click()");

                Thread.Sleep(300);
            }

            if (IsElementVisibile("QuestFrameCompleteQuestButton"))
            {
                ObjectManager.Player.LuaCall("QuestFrameCompleteQuestButton:Click()");

                Thread.Sleep(300);
            }

            if (IsElementVisibile("QuestFrameAcceptButton"))
            {
                ObjectManager.Player.LuaCall("QuestFrameAcceptButton:Click()");

                Thread.Sleep(300);
            }

            if (IsElementVisibile("QuestFrameCloseButton"))
            {
                ObjectManager.Player.LuaCall("QuestFrameCloseButton:Click()");

                Thread.Sleep(300);
            }
        }

        public static string GetQuestGossipOption(int i)
        {
            var hasOption = ObjectManager.Player.LuaCallWithResults($"{{0}} = GossipTitleButton{i}:IsVisible()");
            if (hasOption.Length > 0 && hasOption[0] == "1")
            {
                string[] results = ObjectManager.Player.LuaCallWithResults($"{{0}} = QuestFrameCloseButton{i}:GetText()");
                if (results.Length > 0)
                {
                    return results[0];
                }
            }
            return string.Empty;
        }

        public static string GetQuestTitleOption(int i)
        {
            var hasOption = ObjectManager.Player.LuaCallWithResults($"{{0}} = QuestTitleButton{i}:IsVisible()");
            if (hasOption.Length > 0 && hasOption[0] == "1")
            {
                string[] results = ObjectManager.Player.LuaCallWithResults($"{{0}} = QuestTitleButton{i}:GetText()");
                if (results.Length > 0)
                {
                    return results[0];
                }
            }
            return string.Empty;
        }
        public static bool IsElementVisibile(string elementName)
        {
            var hasOption = ObjectManager.Player.LuaCallWithResults($"{{0}} ={elementName}:IsVisible()");

            return hasOption.Length > 0 && hasOption[0] == "1";
        }
    }
}
