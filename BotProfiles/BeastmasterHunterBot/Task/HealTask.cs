// Friday owns this file!

using RaidMemberBot.AI;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace BeastMasterHunterBot
{
    class HealTask : BotTask, IBotTask
    {
        const string LesserHeal = "Lesser Heal";

        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;
        readonly ulong targetGuid;

        public HealTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Heal) { }

        public void Update()
        {
            //if (ObjectManager.Player.IsCasting) return;

            //if (ObjectManager.Player.HealthPercent > 70 || ObjectManager.Player.Mana < ObjectManager.Player.GetManaCost(LesserHeal))
            //{
            //    ObjectManager.Player.SetTarget(targetGuid);
            //    BotTasks.Pop();
            //    return;
            //}

            //Functions.LuaCall($"CastSpellByName('{LesserHeal}')");
        }
    }
}
