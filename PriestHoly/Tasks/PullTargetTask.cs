using WoWActivityMember.Game.Statics;
using WoWActivityMember.Objects;
using WoWActivityMember.Tasks;

namespace WarlockAffliction.Tasks
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string SummonImp = "Summon Imp";
        const string SummonVoidwalker = "Summon Voidwalker";
        const string CurseOfAgony = "Curse of Agony";
        const string ShadowBolt = "Shadow Bolt";

        readonly string pullingSpell;
        Position currentWaypoint;

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull)
        {
            if (ObjectManager.Player.IsSpellReady(CurseOfAgony))
                pullingSpell = CurseOfAgony;
            else
                pullingSpell = ShadowBolt;
        }

        public void Update()
        {

        }
    }
}
