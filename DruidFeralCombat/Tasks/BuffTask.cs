using WoWActivityMember.Tasks;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;

namespace DruidFeral.Tasks
{
    internal class BuffTask : BotTask, IBotTask
    {
        private const string MarkOfTheWild = "Mark of the Wild";
        private const string Thorns = "Thorns";
        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
        public void Update()
        {
            if ((ObjectManager.Player.HasBuff(MarkOfTheWild) || !ObjectManager.Player.IsSpellReady(MarkOfTheWild)) && (ObjectManager.Player.HasBuff(Thorns) || !ObjectManager.Player.IsSpellReady(Thorns)))
            {
                BotTasks.Pop();
                return;
            }
            
            TryCastSpell(MarkOfTheWild);
            TryCastSpell(Thorns);
        }

        private void TryCastSpell(string name)
        {
            if (!ObjectManager.Player.HasBuff(name) && ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.IsSpellReady(name))
                Functions.LuaCall($"CastSpellByName('{name}',1)");
        }
    }
}
