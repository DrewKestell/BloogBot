using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace ShadowPriestBot
{
    class HealSelfState : IBotState
    {
        const string LesserHeal = "Lesser Heal";
        const string Heal = "Heal";
        const string Renew = "Renew";

        readonly Stack<IBotState> botStates;
        readonly LocalPlayer player;

        readonly string healingSpell;

        public HealSelfState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            player = ObjectManager.Player;

            if (player.KnowsSpell(Heal))
                healingSpell = Heal;
            else
                healingSpell = LesserHeal;
        }

        public void Update()
        {
            if (player.IsCasting) return;

            if (player.HealthPercent > 70 || player.Mana < player.GetManaCost(healingSpell))
            {
                if (player.KnowsSpell(Renew) && player.Mana > player.GetManaCost(Renew))
                    player.LuaCall($"CastSpellByName('{Renew}',1)");

                botStates.Pop();
                return;
            }

            player.LuaCall($"CastSpellByName('{healingSpell}',1)");
        }
    }
}
