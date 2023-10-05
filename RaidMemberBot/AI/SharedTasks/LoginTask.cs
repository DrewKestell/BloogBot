using RaidMemberBot.Game.Statics;
using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
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
            ObjectManager.Instance.AntiAfk();

            if (ObjectManager.Instance.IsIngame)
            {
                botTasks.Pop();
                return;
            }

           if (Login.Instance.LoginState == Constants.Enums.LoginStates.charselect && Login.Instance.GlueDialogText == "Character list retrieved")
            {
                Login.Instance.EnterWorld(characterSlot);
                botTasks.Pop();
            } else if (Login.Instance.LoginState == Constants.Enums.LoginStates.login && string.IsNullOrEmpty(Login.Instance.GlueDialogText))
            {
                Login.Instance.DefaultServerLogin(accountName);
            }
        }
    }
}
