using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using System.Collections.Generic;

namespace AfflictionWarlockBot
{
    class SummonVoidwalkerTask : BotTask, IBotTask
    {
        const string SummonImp = "Summon Imp";
        const string SummonVoidwalker = "Summon Voidwalker";

        public SummonVoidwalkerTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }

        public void Update()
        {
            if (Container.Player.IsCasting)
                return;
            
            if ((!Spellbook.Instance.IsSpellReady(SummonImp) && !Spellbook.Instance.IsSpellReady(SummonVoidwalker)) || ObjectManager.Instance.Pet != null)
            {
                BotTasks.Pop();
                BotTasks.Push(new BuffTask(Container, BotTasks));
                return;
            }

            if (Spellbook.Instance.IsSpellReady(SummonImp))
                Lua.Instance.Execute($"CastSpellByName('{SummonImp}')");
            else
                Lua.Instance.Execute($"CastSpellByName('{SummonVoidwalker}')");
        }
    }
}
