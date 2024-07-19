using WoWSlimClient.Client;
using WoWSlimClient.Models;

namespace WoWSlimClient.Tasks.SharedStates
{
    public class LoginTask : BotTask, IBotTask
    {
        private bool isHandShaking;
        private bool handshakeReset;
        private int lastTickTime;
        private int handshakeDuration;

        public LoginTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary)
        {
            OpCodeDispatcher.Instance.OnWrongLogin += Instance_OnWrongLogin;
            OpCodeDispatcher.Instance.OnHandshakeBegin += Instance_OnHandshakeBegin;
        }

        ~LoginTask()
        {
            OpCodeDispatcher.Instance.OnWrongLogin -= Instance_OnWrongLogin;
            OpCodeDispatcher.Instance.OnHandshakeBegin -= Instance_OnHandshakeBegin;
        }

        private void Instance_OnHandshakeBegin(object sender, EventArgs e)
        {
            Console.WriteLine("[LOGIN TASK] Instance_OnHandshakeBegin");
            isHandShaking = true;
        }

        private void Instance_OnWrongLogin(object sender, EventArgs e)
        {
            Console.WriteLine("[LOGIN TASK] Instance_OnWrongLogin");
            ResetLogin();
        }

        public void Update()
        {
            if (LoginState == Enums.LoginStates.charselect)
            {
                EnterWorld();
                BotTasks.Pop();
            }
            else if (LoginState == Enums.LoginStates.login)
            {
                if (handshakeReset)
                {
                    DefaultServerLogin();

                    handshakeReset = false;
                }
                else
                {
                    if (isHandShaking)
                    {
                        handshakeDuration += Environment.TickCount - lastTickTime;
                        if (handshakeDuration > 2000)
                        {
                            Console.WriteLine("[LOGIN TASK] handshakeDuration > 2000");
                            ResetLogin();

                            isHandShaking = false;
                            handshakeReset = true;
                            handshakeDuration = 0;
                        }
                    }
                }
            }
            lastTickTime = Environment.TickCount;
        }

        private Enums.LoginStates LoginState;

        private int MaxCharacterCount { get; set; }
        public void ResetLogin()
        {
            
        }

        /// <summary>
        ///     Login with the saved credentials
        /// </summary>
        public void DefaultServerLogin()
        {
           
        }

        /// <summary>
        ///     Enters the world.
        /// </summary>
        public void EnterWorld()
        {
            
        }
    }
}
