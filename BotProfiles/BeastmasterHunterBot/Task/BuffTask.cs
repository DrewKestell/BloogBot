// Friday owns this file!

using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using System.Collections.Generic;

namespace BeastMasterHunterBot
{
    class BuffTask : BotTask, IBotTask
    {
        const string AspectOfTheMonkey = "Aspect of the Monkey";
        const string AspectOfTheCheetah = "Aspect of the Cheetah";
        const string AspectOfTheHawk = "Aspect of the Hawk";

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
        public void Update()
        {
            if (!Spellbook.Instance.IsSpellReady(AspectOfTheHawk) || Container.Player.HasBuff(AspectOfTheHawk))
            {
                BotTasks.Pop();
                return;
            }

            TryCastSpell(AspectOfTheHawk);
        }

        void TryCastSpell(string name, int requiredLevel = 1)
        {
            if (!Container.Player.HasBuff(name) && Container.Player.Level >= requiredLevel && Spellbook.Instance.IsSpellReady(name))
                Lua.Instance.Execute($"CastSpellByName('{name}')");
        }
    }
}
