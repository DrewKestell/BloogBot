using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace ShadowPriestBot
{
    class BuffSelfState : IBotState
    {
        const string PowerWordFortitude = "Power Word: Fortitude";
        const string ShadowProtection = "Shadow Protection";

        readonly Stack<IBotState> botStates;
        readonly LocalPlayer player;

        public BuffSelfState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if ((!player.KnowsSpell(PowerWordFortitude) || player.HasBuff(PowerWordFortitude)) && (!player.KnowsSpell(ShadowProtection) || player.HasBuff(ShadowProtection)))
            {
                botStates.Pop();
                return;
            }

            TryCastSpell(PowerWordFortitude);

            TryCastSpell(ShadowProtection);
        }

        void TryCastSpell(string name, int requiredLevel = 1)
        {
            if (!player.HasBuff(name) && player.Level >= requiredLevel && player.IsSpellReady(name))
                player.LuaCall($"CastSpellByName('{name}',1)");
        }
    }
}
