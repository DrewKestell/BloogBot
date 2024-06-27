using WoWActivityMember.Tasks;

namespace WarlockAffliction.Tasks
{
    class BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Buff), IBotTask
    {
        const string DemonArmor = "Demon Armor";
        const string DemonSkin = "Demon Skin";

        public void Update()
        {

        }
    }
}
