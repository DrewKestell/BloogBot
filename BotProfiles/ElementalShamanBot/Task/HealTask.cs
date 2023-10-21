using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using System.Collections.Generic;

namespace ElementalShamanBot
{
    class HealTask : BotTask, IBotTask
    {
        const string WarStomp = "War Stomp";
        const string HealingWave = "Healing Wave";

        public HealTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Heal) { }

        public void Update()
        {
            if (Container.Player.IsCasting) return;

            if (Container.Player.HealthPercent > 70 || Container.Player.Mana < Container.Player.GetManaCost(HealingWave))
            {
                BotTasks.Pop();
                return;
            }

            if (Spellbook.Instance.IsSpellReady(WarStomp))
                Lua.Instance.Execute($"CastSpellByName('{WarStomp}')");

            Lua.Instance.Execute($"CastSpellByName('{HealingWave}',1)");
        }
    }
}
