using WoWActivityMember.Constants;
using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;

namespace WoWActivityMember.Tasks.SharedStates
{
    public class LoginTask : BotTask, IBotTask
    {
        bool isHandShaking;
        bool handshakeReset;

        int lastTickTime;
        int handshakeDuration;

        public LoginTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary)
        {
            WoWEventHandler.Instance.OnWrongLogin += Instance_OnWrongLogin;
            WoWEventHandler.Instance.OnHandshakeBegin += Instance_OnHandshakeBegin;
        }

        ~LoginTask()
        {
            WoWEventHandler.Instance.OnWrongLogin -= Instance_OnWrongLogin;
            WoWEventHandler.Instance.OnHandshakeBegin -= Instance_OnHandshakeBegin;
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
            if (LoginState == Enums.LoginStates.charselect && GlueDialogText == "Character list retrieved" && Wait.For("CharSelectAnim", 500))
            {
                EnterWorld();
                BotTasks.Pop();
            }
            else if (LoginState == Enums.LoginStates.login)
            {
                if (!GlueDialogText.Contains("Handshaking") || handshakeReset)
                {
                    DefaultServerLogin();

                    handshakeReset = false;
                }
                else if (GlueDialogText == "Handshaking")
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

        Enums.LoginStates LoginState => (Enums.LoginStates)Enum.Parse(typeof(Enums.LoginStates), MemoryManager.ReadString(Offsets.CharacterScreen.LoginState));
        string GlueDialogText => Functions.LuaCallWithResult("{0} = GlueDialogText:GetText()")[0];
        int MaxCharacterCount => MemoryManager.ReadInt(0x00B42140);
        public void ResetLogin()
        {
            Functions.LuaCall("arg1 = 'ESCAPE' GlueDialog_OnKeyDown()");
            Functions.LuaCall("if RealmListCancelButton ~= nil then if RealmListCancelButton:IsVisible() then RealmListCancelButton:Click(); end end ");
        }

        /// <summary>
        ///     Login with the saved credentials
        /// </summary>
        public void DefaultServerLogin()
        {
            if (LoginState != Enums.LoginStates.login) return;
            Functions.LuaCall($"DefaultServerLogin('{Container.State.AccountName}', 'password');");
        }

        /// <summary>
        ///     Enters the world.
        /// </summary>
        public void EnterWorld()
        {
            const string str = "if CharSelectEnterWorldButton ~= nil then CharSelectEnterWorldButton:Click()  end";
            Functions.LuaCall(str);
        }
    }
}
