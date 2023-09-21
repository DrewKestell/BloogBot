using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BloogBot.AI.SharedStates
{
    public class LoginState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;

        State state;

        public LoginState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
        }
        public void Update()
        {
            if (IsElementVisibile("AccountLoginAccountEdit"))
            {
                Functions.LuaCall(
                "    local username = 'lrhodes404' " +
                "\r\nlocal password = 'Rockydog1.'" +
                "\r\nAccountLoginAccountEdit:SetText(username)" +
                "\r\nAccountLoginPasswordEdit:SetText(password)" +
                "\r\nAccountLogin_Login()");
            }
            else if (IsElementVisibile("CharacterSelectCharacterFrame"))
            {
                Functions.LuaCall("CharSelectEnterWorldButton:Click()");
            }

        }
        enum State
        {
            Portal,
            Character,
            Loading
        }
        public bool IsElementVisibile(string elementName)
        {
            var hasOption = Functions.LuaCallWithResult($"{{0}} = {elementName}:IsVisible()");

            return hasOption.Length > 0 && hasOption[0] == "1";
        }
    }
}
