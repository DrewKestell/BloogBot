
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Tasks;

namespace WarlockDestruction.Tasks
{
    internal class SummonPetTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Buff), IBotTask
    {
        private const string SummonImp = "Summon Imp";
        private const string SummonVoidwalker = "Summon Voidwalker";

        public void Update()
        {
            if (ObjectManager.Player.IsCasting)
                return;

            ObjectManager.Player.Stand();

            if ((!ObjectManager.Player.IsSpellReady(SummonImp) && !ObjectManager.Player.IsSpellReady(SummonVoidwalker)) || ObjectManager.Pet != null)
            {
                BotTasks.Pop();
                BotTasks.Push(new BuffTask(Container, BotTasks));
                return;
            }

            if (ObjectManager.Player.IsSpellReady(SummonImp))
                Functions.LuaCall($"CastSpellByName('{SummonImp}')");
            else
                Functions.LuaCall($"CastSpellByName('{SummonVoidwalker}')");
        }
    }
}
