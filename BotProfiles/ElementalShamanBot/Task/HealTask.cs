using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
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
            player = ObjectManager.Instance.Player;

            if (Spellbook.Instance.IsSpellReady(WarStomp))
                Lua.Instance.Execute($"CastSpellByName('{WarStomp}')");
        }

        public void Update()
        {
            if (player.IsCasting) return;

            if (player.HealthPercent > 70 || player.Mana < player.GetManaCost(HealingWave))
            {
                botTasks.Pop();
                return;
            }

            Lua.Instance.Execute($"CastSpellByName('{HealingWave}',1)");
        }
    }
}
