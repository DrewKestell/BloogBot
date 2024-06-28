using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Tasks;

namespace WarlockDemonology.Tasks
{
    class SummonPetTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Buff), IBotTask
    {
        const string SummonImp = "Summon Imp";
        const string SummonVoidwalker = "Summon Voidwalker";

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
