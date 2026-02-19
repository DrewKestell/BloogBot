using System.Collections.Generic;
using BloogBot.Game;

namespace BloogBot.AI.SharedStates
{
    public class LoginState : IBotState
    {
        enum State
        {
            LOGIN,
            WAITING_FOR_LOGIN,
            CHARACTER_SELECTION,
            ENTERING_WORLD,
        }

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;

        State state = State.LOGIN;

        public LoginState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
        }

        public void Update()
        {
            switch (state)
            {
                case State.LOGIN:
                    // The disconnected dialog is likely showing. Click it away.
                    if (IsGlueDialogShown())
                    {
                        ClickGlueDialog();
                    }

                    // Start logging in.
                    AttemptLogin();
                    state = State.WAITING_FOR_LOGIN;
                    break;

                case State.WAITING_FOR_LOGIN:
                    // Give it some time to connect.
                    if (Wait.For("LoginDelay", 10 * 1000))
                    {
                        if (GetCurrentGlueScreen() == "charselect")
                        {
                            // We are in.
                            state = State.CHARACTER_SELECTION;
                        }
                        else
                        {
                            // Too slow, try again.
                            state = State.LOGIN;
                        }
                    }
                    break;

                case State.CHARACTER_SELECTION:
                    EnterWorld();
                    state = State.ENTERING_WORLD;
                    break;

                case State.ENTERING_WORLD:
                    // Wait for some time.
                    if (Wait.For("EnterWorldDelay", 10 * 1000))
                    {
                        if (GetCurrentGlueScreen() == "login")
                        {
                            // We disconnected again. Go back to login.
                            state = State.LOGIN;
                        }
                        else if (GetCurrentGlueScreen() == "charselect")
                        {
                            // Somehow we are still at character selection.
                            state = State.CHARACTER_SELECTION;
                        }
                        else
                        {
                            // We are in.
                            botStates.Pop();
                        }
                    }
                    break;

            }
        }

        public static bool ShouldLogin()
        {
            return GetCurrentGlueScreen() == "login";
        }

        static bool IsGlueDialogShown()
        {
            return Functions.LuaCallWithResult(@"
                {0} = GlueDialog:IsShown()
            ")[0] == "1";
        }

        static string GetCurrentGlueScreen()
        {
            return Functions.LuaCallWithResult(@"
                {0} = CURRENT_GLUE_SCREEN
            ")[0];
        }

        static void ClickGlueDialog()
        {
            Functions.LuaCall(@"
                StatusDialogClick()
            ");
        }

        void AttemptLogin()
        {
            Functions.LuaCall($@"
                DefaultServerLogin('{container.BotSettings.Username}', '{container.BotSettings.Password}')
            ");
        }

        static void EnterWorld()
        {
            Functions.LuaCall(@"
                CharacterSelect_EnterWorld()
            ");
        }
    }
}
