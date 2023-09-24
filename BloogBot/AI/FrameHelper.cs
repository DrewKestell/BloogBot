

using BloogBot.Game;

namespace BloogBot.AI
{
    public static class FrameHelper
    {
        public static string GetQuestGossipOption(int i)
        {
            var hasOption = Functions.LuaCallWithResult($"{{0}} = GossipTitleButton{i} ~= nil and GossipTitleButton{i}:IsVisible()");
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
            var hasOption = Functions.LuaCallWithResult($"{{0}} = QuestTitleButton{i} ~= nil and QuestTitleButton{i}:IsVisible()");
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
            if (Functions.LuaCallWithResult($"{{0}} = {elementName} ~= nil and {elementName}:IsVisible()")[0] == "1")
            {
                return float.Parse(Functions.LuaCallWithResult($"{{0}} = {elementName}:GetAlpha()")[0]) == 1;
            }
            return false;
        }
    }
}
