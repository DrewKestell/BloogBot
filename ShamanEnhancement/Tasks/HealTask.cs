using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Tasks;

namespace ShamanEnhancement.Tasks
{
    internal class HealTask : BotTask, IBotTask
    {
        private const string WarStomp = "War Stomp";
        private const string HealingWave = "Healing Wave";
        public HealTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Heal) { }

        public void Update()
        {
            if (ObjectManager.Player.IsCasting) return;

            if (ObjectManager.Player.HealthPercent > 70 || ObjectManager.Player.Mana < ObjectManager.Player.GetManaCost(HealingWave))
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsMoving)
                ObjectManager.Player.StopAllMovement();

            if (ObjectManager.Player.IsSpellReady(WarStomp))
                Functions.LuaCall($"CastSpellByName('{WarStomp}')");

            Functions.LuaCall($"CastSpellByName('{HealingWave}',1)");
        }
    }
}
