using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using System.Collections.Generic;

namespace FeralDruidBot
{
    class HealTask : BotTask, IBotTask
    {
        const string BearForm = "Bear Form";
        const string CatForm = "Cat Form";

        const string WarStomp = "War Stomp";
        const string HealingTouch = "Healing Touch";
        public HealTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Heal) { }

        public void Update()
        {
            if (Container.Player.IsCasting) return;

            if (Container.Player.CurrentShapeshiftForm == BearForm && Wait.For("BearFormDelay", 1000, true))
                CastSpell(BearForm);

            if (Container.Player.CurrentShapeshiftForm == CatForm && Wait.For("CatFormDelay", 1000, true))
                CastSpell(CatForm);

            if (Container.Player.HealthPercent > 70 || Container.Player.Mana < Container.Player.GetManaCost(HealingTouch))
            {
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (Spellbook.Instance.IsSpellReady(WarStomp) && Container.Player.Location.GetDistanceTo(Container.HostileTarget.Location) <= 8)
                Lua.Instance.Execute($"CastSpellByName('{WarStomp}')");

            CastSpell(HealingTouch, castOnSelf: true);
        }

        void CastSpell(string name, bool castOnSelf = false)
        {
            if (Spellbook.Instance.IsSpellReady(name))
            {
                string castOnSelfString = castOnSelf ? ",1" : "";
                Lua.Instance.Execute($"CastSpellByName('{name}'{castOnSelfString})");
            }
        }
    }
}
