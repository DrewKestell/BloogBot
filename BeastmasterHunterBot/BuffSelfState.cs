// Friday owns this file!

using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace BeastMasterHunterBot
{
    class BuffSelfState : IBotState
    {
        const string AspectOfTheMonkey = "Aspect of the Monkey";
        const string AspectOfTheCheetah = "Aspect of the Cheetah";
        const string AspectOfTheHawk = "Aspect of the Hawk";

        readonly Stack<IBotState> botStates;
        readonly LocalPlayer player;

        public BuffSelfState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            player = ObjectManager.Player;
            player.SetTarget(player.Guid);
        }

        public void Update()
        {
            if (!player.KnowsSpell(AspectOfTheHawk) || player.HasBuff(AspectOfTheHawk))
            {
                botStates.Pop();
                return;
            }

            TryCastSpell(AspectOfTheHawk);
        }

        void TryCastSpell(string name, int requiredLevel = 1)
        {
            if (!player.HasBuff(name) && player.Level >= requiredLevel && player.IsSpellReady(name))
                player.LuaCall($"CastSpellByName('{name}')");
        }
    }
}
