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
            //if (Container.Player.IsCasting) return;

            //if (Container.Player.HealthPercent > 70 || Container.Player.Mana < Container.Player.GetManaCost(LesserHeal))
            //{
            //    Container.Player.SetTarget(targetGuid);
            //    BotTasks.Pop();
            //    return;
            //}

            //Lua.Instance.Execute($"CastSpellByName('{LesserHeal}')");
        }
    }
}
