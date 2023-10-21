using RaidMemberBot.Game.Statics;
using System;
using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
{
    public class LoginTask : BotTask, IBotTask
    {
        string accountName;
        int characterSlot;

        public LoginTask(IClassContainer container, Stack<IBotTask> botTasks, string accountName, int characterSlot) : base(container, botTasks, TaskType.Ordinary)
        {
            this.accountName = accountName;
            this.characterSlot = characterSlot;

            WoWEventHandler.Instance.OnWrongLogin += Instance_OnWrongLogin;
        }

        ~LoginTask() {
            WoWEventHandler.Instance.OnWrongLogin -= Instance_OnWrongLogin;
        }

        private void Instance_OnWrongLogin(object sender, EventArgs e)
        {
            Login.Instance.ResetLogin();
        }

        public void Update()
        {
            if (ObjectManager.Instance.IsIngame)
            {
                BotTasks.Pop();
                return;
            }

           if (Login.Instance.LoginState == Constants.Enums.LoginStates.charselect && Login.Instance.GlueDialogText == "Character list retrieved")
            {
                Login.Instance.EnterWorld(characterSlot);
                BotTasks.Pop();
            } else if (Login.Instance.LoginState == Constants.Enums.LoginStates.login && string.IsNullOrEmpty(Login.Instance.GlueDialogText))
            {
                Login.Instance.DefaultServerLogin(accountName);
            }
        }
    }
}
