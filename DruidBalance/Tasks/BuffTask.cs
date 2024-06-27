using WoWActivityMember.Tasks;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;

namespace DruidBalance.Tasks
{
    class BuffTask : BotTask, IBotTask
    {
        const string MarkOfTheWild = "Mark of the Wild";
        const string Thorns = "Thorns";
        const string OmenOfClarity = "Omen of Clarity";
        const string MoonkinForm = "Moonkin Form";

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
        public void Update()
        {
            if ((ObjectManager.Player.HasBuff(MarkOfTheWild) || !ObjectManager.Player.IsSpellReady(MarkOfTheWild)) &&
                (ObjectManager.Player.HasBuff(Thorns) || !ObjectManager.Player.IsSpellReady(Thorns)) &&
                (ObjectManager.Player.HasBuff(OmenOfClarity) || !ObjectManager.Player.IsSpellReady(OmenOfClarity)))
            {
                BotTasks.Pop();
                return;
            }

            if (!ObjectManager.Player.HasBuff(MarkOfTheWild))
            {
                if (ObjectManager.Player.HasBuff(MoonkinForm))
                {
                    Functions.LuaCall($"CastSpellByName('{MoonkinForm}')");
                }

                TryCastSpell(MarkOfTheWild);
            }

            TryCastSpell(Thorns);
            TryCastSpell(OmenOfClarity);
        }

        void TryCastSpell(string name)
        {
            if (!ObjectManager.Player.HasBuff(name) && ObjectManager.Player.IsSpellReady(name))
                Functions.LuaCall($"CastSpellByName('{name}',1)");
        }
    }
}
