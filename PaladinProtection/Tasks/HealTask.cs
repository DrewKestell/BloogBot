using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Tasks;

namespace PaladinProtection
{
    internal class HealTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Heal), IBotTask
    {
        private const string DivineProtection = "Divine Protection";
        private const string HolyLight = "Holy Light";

        public void Update()
        {
            if (ObjectManager.Player.IsCasting) return;

            if (ObjectManager.Player.HealthPercent > 70 || ObjectManager.Player.Mana < ObjectManager.Player.GetManaCost(HolyLight))
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Mana > ObjectManager.Player.GetManaCost(DivineProtection) && ObjectManager.Player.IsSpellReady(DivineProtection))
                Functions.LuaCall($"CastSpellByName('{DivineProtection}')");

            if (ObjectManager.Player.Mana > ObjectManager.Player.GetManaCost(HolyLight) && ObjectManager.Player.IsSpellReady(HolyLight))
            {
                Functions.LuaCall($"CastSpellByName(\"HolyLight\",1)");
            }
        }
    }
}
