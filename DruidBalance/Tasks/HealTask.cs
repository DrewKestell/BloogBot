using WoWActivityMember.Tasks;
using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;

namespace DruidBalance.Tasks
{
    internal class HealTask : BotTask, IBotTask
    {
        private const string WarStomp = "War Stomp";
        private const string HealingTouch = "Healing Touch";
        private const string Rejuvenation = "Rejuvenation";
        private const string Barkskin = "Barkskin";
        private const string MoonkinForm = "Moonkin Form";

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

            if (ObjectManager.Player.IsSpellReady(WarStomp) && ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) <= 8)
                Functions.LuaCall($"CastSpellByName('{WarStomp}')");

            TryCastSpell(MoonkinForm, ObjectManager.Player.HasBuff(MoonkinForm));

            TryCastSpell(Barkskin);

            TryCastSpell(Rejuvenation, !ObjectManager.Player.HasBuff(Rejuvenation));

            TryCastSpell(HealingTouch);
        }

        private void TryCastSpell(string name, bool condition = true)
        {
            if (ObjectManager.Player.IsSpellReady(name) && condition)
                Functions.LuaCall($"CastSpellByName('{name}',1)");
        }
    }
}
