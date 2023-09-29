using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace ElementalShamanBot
{
    class HealTask : IBotTask
    {
        const string WarStomp = "War Stomp";
        const string HealingWave = "Healing Wave";

        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;

        public HealTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.botTasks = botTasks;
            player = ObjectManager.Player;

            if (player.IsSpellReady(WarStomp))
                player.LuaCall($"CastSpellByName('{WarStomp}')");
        }

        public void Update()
        {
            if (player.IsCasting) return;

            if (player.HealthPercent > 70 || player.Mana < player.GetManaCost(HealingWave))
            {
                botTasks.Pop();
                return;
            }

            player.LuaCall($"CastSpellByName('{HealingWave}',1)");
        }
    }
}
