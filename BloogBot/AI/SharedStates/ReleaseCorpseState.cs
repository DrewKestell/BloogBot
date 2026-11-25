using BloogBot.Game;
using System.Collections.Generic;

namespace BloogBot.AI.SharedStates
{
    public class ReleaseCorpseState : IBotState
    {
        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;

        bool triedSoulstone;

        public ReleaseCorpseState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
        }

        public void Update()
        {
            if (Wait.For("StartReleaseCorpseStateDelay", 1000))
            {
                if (ObjectManager.Player.InGhostForm || ObjectManager.Player.IsAlive)
                {
                    if (Wait.For("LeaveReleaseCorpseStateDelay", 2000))
                    {
                        botStates.Pop();
                        return;
                    }
                }
                else
                {
                    if (!triedSoulstone)
                    {
                        triedSoulstone = true;
                        var hasSoulstone = Functions.LuaCallWithResult("{0} = HasSoulstone()");
                        if (hasSoulstone.Length > 0 && !string.IsNullOrEmpty(hasSoulstone[0]))
                        {
                            Functions.LuaCall("UseSoulstone()");
                            return;
                        }
                    }

                    ObjectManager.Player.ReleaseCorpse();
                }
            }
        }
    }
}
