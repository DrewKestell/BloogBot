using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using System;
using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
{
    public class LoginTask : BotTask, IBotTask
    {
        string accountName;

        bool isHandShaking;

        int lastTickTime;
        int handshakeDuration;

        public LoginTask(IClassContainer container, Stack<IBotTask> botTasks, string accountName) : base(container, botTasks, TaskType.Ordinary)
        {
            this.accountName = accountName;

            WoWEventHandler.Instance.OnWrongLogin += Instance_OnWrongLogin;
            WoWEventHandler.Instance.OnHandshakeBegin += Instance_OnHandshakeBegin;
        }

        private void Instance_OnHandshakeBegin(object sender, EventArgs e)
        {
            isHandShaking = true;
        }

        ~LoginTask()
        {
            WoWEventHandler.Instance.OnWrongLogin -= Instance_OnWrongLogin;
        }

        private void Instance_OnWrongLogin(object sender, EventArgs e)
        {
            Console.WriteLine($"Instance_OnWrongLogin");
            Login.Instance.ResetLogin();
        }

        public void Update()
        {
            if (Login.Instance.LoginState == Constants.Enums.LoginStates.charselect && Login.Instance.GlueDialogText == "Character list retrieved" && Wait.For("CharSelectAnim", 500))
            {
                Login.Instance.EnterWorld(0);
                BotTasks.Pop();
            }
            else if (Login.Instance.LoginState == Constants.Enums.LoginStates.login && string.IsNullOrEmpty(Login.Instance.GlueDialogText))
            {
                if (string.IsNullOrEmpty(Login.Instance.GlueDialogText))
                {
                    Login.Instance.DefaultServerLogin(accountName);
                }
                else if (Login.Instance.GlueDialogText == "Handshaking")
                {
                    if (isHandShaking && Login.Instance.GlueDialogText == "Handshaking")
                    {
                        handshakeDuration += Environment.TickCount - lastTickTime;
                        if (handshakeDuration > 2000)
                        {
                            Console.WriteLine($"handshakeDuration > 2000");
                            Login.Instance.ResetLogin();
                        }
                    }
                }
            }
            lastTickTime = Environment.TickCount;
        }
    }
}
