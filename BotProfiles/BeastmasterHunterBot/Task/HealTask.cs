// Friday owns this file!

using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace BeastMasterHunterBot
{
    class HealTask : IBotTask
    {
        const string LesserHeal = "Lesser Heal";

        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;
        readonly ulong targetGuid;

        public HealTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.botTasks = botTasks;
            player = ObjectManager.Instance.Player;
            targetGuid = player.TargetGuid;
            player.SetTarget(player.Guid);
        }

        public void Update()
        {
            //if (player.IsCasting) return;

            //if (player.HealthPercent > 70 || player.Mana < player.GetManaCost(LesserHeal))
            //{
            //    player.SetTarget(targetGuid);
            //    botTasks.Pop();
            //    return;
            //}

            //Lua.Instance.Execute($"CastSpellByName('{LesserHeal}')");
        }
    }
}
