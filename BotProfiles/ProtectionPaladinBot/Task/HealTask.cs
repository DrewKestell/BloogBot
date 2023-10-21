using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using System.Collections.Generic;

namespace ProtectionPaladinBot
{
    class HealTask : BotTask, IBotTask
    {
        const string DivineProtection = "Divine Protection";
        const string HolyLight = "Holy Light";
        public HealTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Heal) { }

        public void Update()
        {
            if (Container.Player.IsCasting) return;

            if (Container.Player.HealthPercent > 70 || Container.Player.Mana < Container.Player.GetManaCost(HolyLight))
            {
                BotTasks.Pop();
                return;
            }

            if (Container.Player.Mana > Container.Player.GetManaCost(DivineProtection) && Spellbook.Instance.IsSpellReady(DivineProtection))
                Lua.Instance.Execute($"CastSpellByName('{DivineProtection}')");

            if (Container.Player.Mana > Container.Player.GetManaCost(HolyLight) && Spellbook.Instance.IsSpellReady(HolyLight))
            {
                Lua.Instance.Execute($"CastSpellByName(\"HolyLight\",1)");
            }
        }
    }
}
