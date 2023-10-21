using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using System.Collections.Generic;

namespace BalanceDruidBot
{
    class BuffTask : BotTask, IBotTask
    {
        const string MarkOfTheWild = "Mark of the Wild";
        const string Thorns = "Thorns";
        const string OmenOfClarity = "Omen of Clarity";
        const string MoonkinForm = "Moonkin Form";

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
        public void Update()
        {
            if ((Container.Player.HasBuff(MarkOfTheWild) || !Spellbook.Instance.IsSpellReady(MarkOfTheWild)) &&
                (Container.Player.HasBuff(Thorns) || !Spellbook.Instance.IsSpellReady(Thorns)) &&
                (Container.Player.HasBuff(OmenOfClarity) || !Spellbook.Instance.IsSpellReady(OmenOfClarity)))
            {
                BotTasks.Pop();
                return;
            }

            if (!Container.Player.HasBuff(MarkOfTheWild))
            {
                if (Container.Player.HasBuff(MoonkinForm))
                {
                    Lua.Instance.Execute($"CastSpellByName('{MoonkinForm}')");
                }

                TryCastSpell(MarkOfTheWild);
            }

            TryCastSpell(Thorns);
            TryCastSpell(OmenOfClarity);
        }

        void TryCastSpell(string name)
        {
            if (!Container.Player.HasBuff(name) && Spellbook.Instance.IsSpellReady(name))
                Lua.Instance.Execute($"CastSpellByName('{name}',1)");
        }
    }
}
