using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace EnhancementShamanBot
{
    class HealSelfState : IBotState
    {
        const string WarStomp = "War Stomp";
        const string HealingWave = "Healing Wave";

        readonly Stack<IBotState> botStates;
        readonly LocalPlayer player;

        public HealSelfState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            player = ObjectManager.Player;

            if (player.IsSpellReady(WarStomp))
                player.LuaCall($"CastSpellByName('{WarStomp}')");
        }

        public void Update()
        {
            if (player.IsCasting) return;

            if (player.HealthPercent > 70 || player.Mana < player.GetManaCost(HealingWave))
            {
                botStates.Pop();
                return;
            }

            player.LuaCall($"CastSpellByName('{HealingWave}',1)");
        }
    }
}
