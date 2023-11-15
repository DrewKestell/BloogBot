using RaidMemberBot.AI;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Mem;
using System.Collections.Generic;

namespace BalanceDruidBot
{
    class HealTask : BotTask, IBotTask
    {
        const string WarStomp = "War Stomp";
        const string HealingTouch = "Healing Touch";
        const string Rejuvenation = "Rejuvenation";
        const string Barkskin = "Barkskin";
        const string MoonkinForm = "Moonkin Form";

        public HealTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Heal) { }

        public void Update()
        {
            if (ObjectManager.Player.IsCasting) return;

            if (ObjectManager.Player.HealthPercent > 70 || (ObjectManager.Player.Mana < ObjectManager.Player.GetManaCost(HealingTouch) && ObjectManager.Player.Mana < ObjectManager.Player.GetManaCost(Rejuvenation)))
            {
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsSpellReady(WarStomp) && ObjectManager.Player.Position.DistanceTo(Container.HostileTarget.Position) <= 8)
                Functions.LuaCall($"CastSpellByName('{WarStomp}')");

            TryCastSpell(MoonkinForm, ObjectManager.Player.HasBuff(MoonkinForm));

            TryCastSpell(Barkskin);

            TryCastSpell(Rejuvenation, !ObjectManager.Player.HasBuff(Rejuvenation));

            TryCastSpell(HealingTouch);
        }

        void TryCastSpell(string name, bool condition = true)
        {
            if (ObjectManager.Player.IsSpellReady(name) && condition)
                Functions.LuaCall($"CastSpellByName('{name}',1)");
        }
    }
}
