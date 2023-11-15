// Friday owns this file!

using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
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
            if (!ObjectManager.Player.IsSpellReady(AspectOfTheHawk) || ObjectManager.Player.HasBuff(AspectOfTheHawk))
            {
                BotTasks.Pop();
                return;
            }

            TryCastSpell(AspectOfTheHawk);
        }

        void TryCastSpell(string name, int requiredLevel = 1)
        {
            if (!ObjectManager.Player.HasBuff(name) && ObjectManager.Player.Level >= requiredLevel && ObjectManager.Player.IsSpellReady(name))
                Functions.LuaCall($"CastSpellByName('{name}')");
        }
    }
}
