using BloogBot.Game;
using System.Collections.Generic;

namespace BloogBot.AI.SharedStates
{
    public class LoginTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;

        string accountName;
        int characterSlot;

        public LoginTask(IClassContainer container, Stack<IBotTask> botTasks, string accountName, int characterSlot)
        {
            this.botTasks = botTasks;
            this.container = container;
            this.accountName = accountName;
            this.characterSlot = characterSlot;
        }
        public void Update()
        {
            if (FrameHelper.IsElementVisibile("CharSelectEnterWorldButton") && Wait.For("CharacterScreenAnim", 1000))
            {
                Functions.LuaCall("CharSelectEnterWorldButton:Click()");
                botTasks.Pop();
            }
            else if (FrameHelper.IsElementVisibile("AccountLoginAccountEdit"))
            {
                Functions.LuaCall(
                $"   local username = '{accountName}'" +
                "\r\nlocal password = 'password'" +
                "\r\nAccountLoginAccountEdit:SetText(username)" +
                "\r\nAccountLoginPasswordEdit:SetText(password)" +
                "\r\nAccountLogin_Login()");
            }
        }
    }
}
