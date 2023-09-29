using BloogBot.Game;
using System.Collections.Generic;

namespace BloogBot.AI.SharedStates
{
    public class LogoutTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;

        public LogoutTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
        }
        public void Update()
        {
            if (FrameHelper.IsElementVisibile("AccountLoginAccountEdit"))
            {
                botTasks.Pop();
            }
            else if (FrameHelper.IsElementVisibile("CharacterSelectBackButton"))
            {
                Functions.LuaCall("CharacterSelectBackButton:Click()");
            }
            else if (FrameHelper.IsElementVisibile("PlayerFrame"))
            {
                Functions.LuaCall("Logout()");
            }
        }
    }
}
