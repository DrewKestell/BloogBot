using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace RetributionPaladinBot
{
    class BuffSelfState : IBotState
    {
        const string BlessingOfKings = "Blessing of Kings";
        const string BlessingOfMight = "Blessing of Might";
        const string BlessingOfSanctuary = "Blessing of Sanctuary";

        readonly Stack<IBotState> botStates;
        readonly LocalPlayer player;

        public BuffSelfState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if (!player.KnowsSpell(BlessingOfMight) || player.HasBuff(BlessingOfMight) || player.HasBuff(BlessingOfKings) || player.HasBuff(BlessingOfSanctuary))
            {
                botStates.Pop();
                return;
            }
            
            if (player.KnowsSpell(BlessingOfMight) && !player.KnowsSpell(BlessingOfKings) && !player.KnowsSpell(BlessingOfSanctuary))
                TryCastSpell(BlessingOfMight);

            if (player.KnowsSpell(BlessingOfKings) && !player.KnowsSpell(BlessingOfSanctuary))
                TryCastSpell(BlessingOfKings);
            
            if (player.KnowsSpell(BlessingOfSanctuary))
                TryCastSpell(BlessingOfSanctuary);
        }

        void TryCastSpell(string name)
        {
            if (!player.HasBuff(name) && player.IsSpellReady(name) && player.Mana > player.GetManaCost(name))
                player.LuaCall($"CastSpellByName('{name}',1)");
        }
    }
}
