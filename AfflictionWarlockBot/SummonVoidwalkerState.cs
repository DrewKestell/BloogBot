using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace AfflictionWarlockBot
{
    class SummonVoidwalkerState : IBotState
    {
        const string SummonImp = "Summon Imp";
        const string SummonVoidwalker = "Summon Voidwalker";
        
        readonly Stack<IBotState> botStates;
        readonly LocalPlayer player;

        public SummonVoidwalkerState(Stack<IBotState> botStates)
        {
            this.botStates = botStates;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if (player.IsCasting)
                return;

            
            if ((!player.KnowsSpell(SummonImp) && !player.KnowsSpell(SummonVoidwalker)) || ObjectManager.Pet != null)
            {
                botStates.Pop();
                botStates.Push(new BuffSelfState(botStates));
                return;
            }

            if (player.KnowsSpell(SummonVoidwalker))
                player.LuaCall($"CastSpellByName('{SummonVoidwalker}')");
            else
                player.LuaCall($"CastSpellByName('{SummonImp}')");
        }
    }
}
