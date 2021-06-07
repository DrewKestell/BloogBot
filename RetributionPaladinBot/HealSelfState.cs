using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace RetributionPaladinBot
{
    class HealSelfState : IBotState
    {
        const string DivineProtection = "Divine Protection";
        const string HolyLight = "Holy Light";

        readonly Stack<IBotState> botStates;
        readonly LocalPlayer player;

        public HealSelfState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if (player.IsCasting) return;

            if (player.HealthPercent > 70 || player.Mana < player.GetManaCost(HolyLight))
            {
                botStates.Pop();
                return;
            }

            if (player.Mana > player.GetManaCost(DivineProtection) && player.IsSpellReady(DivineProtection))
                player.LuaCall($"CastSpellByName('{DivineProtection}')");

            if (player.Mana > player.GetManaCost(HolyLight) && player.IsSpellReady(HolyLight))
                player.LuaCall($"CastSpellByName('{HolyLight}',1)");
        }
    }
}
