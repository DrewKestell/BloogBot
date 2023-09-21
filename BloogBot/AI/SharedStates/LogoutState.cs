using BloogBot.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloogBot.AI.SharedStates
{
    public class LogoutState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;

        State state;

        public LogoutState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
        }
        public void Update()
        {
            Functions.LuaCall("/logout");

            if (IsElementVisibile("AccountLoginAccountEdit"))
            {
                botStates.Pop();
            }
        }
        public bool IsElementVisibile(string elementName)
        {
            var hasOption = Functions.LuaCallWithResult($"{{0}} = {elementName}:IsVisible()");

            return hasOption.Length > 0 && hasOption[0] == "1";
        }
    }
}
