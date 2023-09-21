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

        string accountUsername;

        public LoginState(Stack<IBotState> botStates, IDependencyContainer container, string username)
        {
            this.botStates = botStates;
            this.container = container;
            this.accountUsername = username;
        }
        public void Update()
        {
            if (IsElementVisibile("AccountLoginAccountEdit"))
            {
                Functions.LuaCall(
                $"    local username = '{accountUsername}' " +
                "\r\nlocal password = 'password'" +
                "\r\nAccountLoginAccountEdit:SetText(username)" +
                "\r\nAccountLoginPasswordEdit:SetText(password)" +
                "\r\nAccountLogin_Login()");
            }
            else if (IsElementVisibile("CharacterSelectCharacterFrame"))
            {
                Functions.LuaCall("CharSelectEnterWorldButton:Click()");
            }

        }
        public bool IsElementVisibile(string elementName)
        {
            var hasOption = Functions.LuaCallWithResult($"{{0}} = {elementName}:IsVisible()");

            return hasOption.Length > 0 && hasOption[0] == "1";
        }
    }
}
