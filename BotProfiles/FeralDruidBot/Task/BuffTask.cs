using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using System.Collections.Generic;

namespace FeralDruidBot
{
    class BuffTask : BotTask, IBotTask
    {
        const string MarkOfTheWild = "Mark of the Wild";
        const string Thorns = "Thorns";
        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
        public void Update()
        {
            if ((Container.Player.HasBuff(MarkOfTheWild) || !Spellbook.Instance.IsSpellReady(MarkOfTheWild)) && (Container.Player.HasBuff(Thorns) || !Spellbook.Instance.IsSpellReady(Thorns)))
            {
                BotTasks.Pop();
                return;
            }
            
            TryCastSpell(MarkOfTheWild);
            TryCastSpell(Thorns);
        }

        void TryCastSpell(string name)
        {
            if (!Container.Player.HasBuff(name) && Spellbook.Instance.IsSpellReady(name) && Spellbook.Instance.IsSpellReady(name))
                Lua.Instance.Execute($"CastSpellByName('{name}',1)");
        }
    }
}
