// Friday owns this file!

using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
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
            player = ObjectManager.Player;
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

            //player.LuaCall($"CastSpellByName('{LesserHeal}')");
        }
    }
}
