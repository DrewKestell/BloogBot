using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
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
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if (player.Casting > 0) return;

            if (player.HealthPercent > 70 || player.Mana < player.GetManaCost(HolyLight))
            {
                botTasks.Pop();
                return;
            }

            if (player.Mana > player.GetManaCost(DivineProtection) && Spellbook.Instance.IsSpellReady(DivineProtection))
                Lua.Instance.Execute($"CastSpellByName('{DivineProtection}')");

            if (player.Mana > player.GetManaCost(HolyLight) && Spellbook.Instance.IsSpellReady(HolyLight))
            {
                Lua.Instance.Execute($"CastSpellByName(\"HolyLight\",1)");
            }
        }
    }
}
