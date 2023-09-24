using BloogBot.Game;
using System.Collections.Generic;

namespace BloogBot.AI.SharedStates
{
    public class LogoutState : BotState, IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;

        public LogoutState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
        }
        public void Update()
        {
            if (FrameHelper.IsElementVisibile("AccountLoginAccountEdit"))
            {
                botStates.Pop();
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
