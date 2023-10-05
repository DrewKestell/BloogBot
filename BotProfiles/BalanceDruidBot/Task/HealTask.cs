using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace BalanceDruidBot
{
    class HealTask : IBotTask
    {
        const string WarStomp = "War Stomp";
        const string HealingTouch = "Healing Touch";
        const string Rejuvenation = "Rejuvenation";
        const string Barkskin = "Barkskin";
        const string MoonkinForm = "Moonkin Form";

        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly WoWUnit target;
        readonly LocalPlayer player;

        public HealTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.target = target;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if (player.IsCasting) return;

            if (player.HealthPercent > 70 || (player.Mana < player.GetManaCost(HealingTouch) && player.Mana < player.GetManaCost(Rejuvenation)))
            {
                Wait.RemoveAll();
                botTasks.Pop();
                return;
            }

            if (Spellbook.Instance.IsSpellReady(WarStomp) && player.Location.GetDistanceTo(target.Location) <= 8)
                Lua.Instance.Execute($"CastSpellByName('{WarStomp}')");

            TryCastSpell(MoonkinForm, player.HasBuff(MoonkinForm));

            TryCastSpell(Barkskin);

            TryCastSpell(Rejuvenation, !player.HasBuff(Rejuvenation));

            TryCastSpell(HealingTouch);
        }

        void TryCastSpell(string name, bool condition = true)
        {
            if (Spellbook.Instance.IsSpellReady(name) && condition)
                Lua.Instance.Execute($"CastSpellByName('{name}',1)");
        }
    }
}
