using BotRunner.Constants;
using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    public class LoginTask : BotTask, IBotTask
    {
        private bool isHandShaking;
        private bool handshakeReset;
        private int lastTickTime;
        private int handshakeDuration;

        public LoginTask(IBotContext botContext) : base(botContext)
        {
            EventHandler.OnLoginFailure += Instance_OnWrongLogin;
            EventHandler.OnHandshakeBegin += Instance_OnHandshakeBegin;
        }

        ~LoginTask()
        {
            EventHandler.OnLoginFailure -= Instance_OnWrongLogin;
            EventHandler.OnHandshakeBegin -= Instance_OnHandshakeBegin;
        }

        private void Instance_OnHandshakeBegin(object sender, EventArgs e)
        {
            Console.WriteLine("[LOGIN TASK] Instance_OnHandshakeBegin");
            isHandShaking = true;
        }

        private void Instance_OnWrongLogin(object sender, EventArgs e)
        {
            Console.WriteLine("[LOGIN TASK] Instance_OnWrongLogin");
            ObjectManager.ResetLogin();
        }

        public void Update()
        {
            if (ObjectManager.LoginState == LoginStates.charselect && ObjectManager.GlueDialogText == "Character list retrieved" && Wait.For("CharSelectAnim", 500))
            {
                ObjectManager.EnterWorld();
                BotTasks.Pop();
            }
            else if (ObjectManager.LoginState == LoginStates.login)
            {
                if (!ObjectManager.GlueDialogText.Contains("Handshaking") || handshakeReset)
                {
                    ObjectManager.DefaultServerLogin(Container.State.Member.AccountName, "password");

                    handshakeReset = false;
                }
                else if (ObjectManager.GlueDialogText == "Handshaking")
                {
                    if (isHandShaking)
                    {
                        handshakeDuration += Environment.TickCount - lastTickTime;
                        if (handshakeDuration > 2000)
                        {
                            Console.WriteLine("[LOGIN TASK] handshakeDuration > 2000");
                            ObjectManager.ResetLogin();

                            isHandShaking = false;
                            handshakeReset = true;
                            handshakeDuration = 0;
                        }
                    }
                }
            }
            lastTickTime = Environment.TickCount;
        }
    }
}
