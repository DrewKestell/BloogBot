// Friday owns this file!

using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace BeastMasterHunterBot
{
    class HealSelfState : IBotState
    {
        const string LesserHeal = "Lesser Heal";

        readonly Stack<IBotState> botStates;
        readonly LocalPlayer player;
        readonly ulong targetGuid;

        public HealSelfState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
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
            //    botStates.Pop();
            //    return;
            //}

            //player.LuaCall($"CastSpellByName('{LesserHeal}')");
        }
    }
}
