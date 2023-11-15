using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
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
            if ((ObjectManager.Player.HasBuff(MarkOfTheWild) || !ObjectManager.Player.IsSpellReady(MarkOfTheWild)) && (ObjectManager.Player.HasBuff(Thorns) || !ObjectManager.Player.IsSpellReady(Thorns)))
            {
                BotTasks.Pop();
                return;
            }
            
            TryCastSpell(MarkOfTheWild);
            TryCastSpell(Thorns);
        }

        void TryCastSpell(string name)
        {
            if (!ObjectManager.Player.HasBuff(name) && ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.IsSpellReady(name))
                Functions.LuaCall($"CastSpellByName('{name}',1)");
        }
    }
}
