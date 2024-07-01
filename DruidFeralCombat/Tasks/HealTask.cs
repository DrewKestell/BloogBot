using WoWActivityMember.Tasks;
using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;

namespace DruidFeral.Tasks
{
    internal class HealTask : BotTask, IBotTask
    {
        private const string BearForm = "Bear Form";
        private const string CatForm = "Cat Form";
        private const string WarStomp = "War Stomp";
        private const string HealingTouch = "Healing Touch";
        public HealTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Heal) { }

        public void Update()
        {
            if (ObjectManager.Player.IsCasting) return;

            if (ObjectManager.Player.CurrentShapeshiftForm == BearForm && Wait.For("BearFormDelay", 1000, true))
                CastSpell(BearForm);

            if (ObjectManager.Player.CurrentShapeshiftForm == CatForm && Wait.For("CatFormDelay", 1000, true))
                CastSpell(CatForm);

            if (ObjectManager.Player.HealthPercent > 70 || ObjectManager.Player.Mana < ObjectManager.Player.GetManaCost(HealingTouch))
            {
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsSpellReady(WarStomp) && ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) <= 8)
                Functions.LuaCall($"CastSpellByName('{WarStomp}')");

            CastSpell(HealingTouch, castOnSelf: true);
        }

        private void CastSpell(string name, bool castOnSelf = false)
        {
            if (ObjectManager.Player.IsSpellReady(name))
            {
                string castOnSelfString = castOnSelf ? ",1" : "";
                Functions.LuaCall($"CastSpellByName('{name}'{castOnSelfString})");
            }
        }
    }
}
