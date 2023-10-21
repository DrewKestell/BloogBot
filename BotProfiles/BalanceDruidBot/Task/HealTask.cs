using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using System.Collections.Generic;

namespace BalanceDruidBot
{
    class HealTask : BotTask, IBotTask
    {
        const string WarStomp = "War Stomp";
        const string HealingTouch = "Healing Touch";
        const string Rejuvenation = "Rejuvenation";
        const string Barkskin = "Barkskin";
        const string MoonkinForm = "Moonkin Form";

        public HealTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Heal) { }

        public void Update()
        {
            if (Container.Player.IsCasting) return;

            if (Container.Player.HealthPercent > 70 || (Container.Player.Mana < Container.Player.GetManaCost(HealingTouch) && Container.Player.Mana < Container.Player.GetManaCost(Rejuvenation)))
            {
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (Spellbook.Instance.IsSpellReady(WarStomp) && Container.Player.Location.GetDistanceTo(Container.HostileTarget.Location) <= 8)
                Lua.Instance.Execute($"CastSpellByName('{WarStomp}')");

            TryCastSpell(MoonkinForm, Container.Player.HasBuff(MoonkinForm));

            TryCastSpell(Barkskin);

            TryCastSpell(Rejuvenation, !Container.Player.HasBuff(Rejuvenation));

            TryCastSpell(HealingTouch);
        }

        void TryCastSpell(string name, bool condition = true)
        {
            if (Spellbook.Instance.IsSpellReady(name) && condition)
                Lua.Instance.Execute($"CastSpellByName('{name}',1)");
        }
    }
}
