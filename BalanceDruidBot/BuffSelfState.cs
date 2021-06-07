using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace BalanceDruidBot
{
    class BuffSelfState : IBotState
    {
        const string MarkOfTheWild = "Mark of the Wild";
        const string Thorns = "Thorns";

        readonly Stack<IBotState> botStates;
        readonly LocalPlayer player;

        public BuffSelfState(Stack<IBotState> botStates)
        {
            this.botStates = botStates;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if ((player.HasBuff(MarkOfTheWild) || !player.KnowsSpell(MarkOfTheWild)) && (player.HasBuff(Thorns) || !player.KnowsSpell(Thorns)))
            {
                botStates.Pop();
                return;
            }
            
            TryCastSpell(MarkOfTheWild);
            TryCastSpell(Thorns);
        }

        void TryCastSpell(string name)
        {
            if (!player.HasBuff(name) && player.IsSpellReady(name))
                player.LuaCall($"CastSpellByName('{name}',1)");
        }
    }
}
