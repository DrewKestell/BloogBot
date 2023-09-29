using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace ProtectionPaladinBot
{
    class HealTask : IBotTask
    {
        const string DivineProtection = "Divine Protection";
        const string HolyLight = "Holy Light";

        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;

        public HealTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.botTasks = botTasks;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if (player.IsCasting) return;

            if (player.HealthPercent > 70 || player.Mana < player.GetManaCost(HolyLight))
            {
                botTasks.Pop();
                return;
            }

            if (player.Mana > player.GetManaCost(DivineProtection) && player.IsSpellReady(DivineProtection))
                player.LuaCall($"CastSpellByName('{DivineProtection}')");

            if (player.Mana > player.GetManaCost(HolyLight) && player.IsSpellReady(HolyLight))
            {
                if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
                {
                    player.LuaCall($"CastSpellByName(\"HolyLight\",1)");
                }
                else
                {
                    player.CastSpell(HolyLight, player.Guid);
                }
            }
        }
    }
}
