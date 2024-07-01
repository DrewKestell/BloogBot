using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;

namespace WoWActivityMember.Tasks.SharedStates
{
    public class AddSpellTask(IClassContainer container, Stack<IBotTask> botTasks, int spellId) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private readonly int spellId = spellId;

        public void Update()
        {
            if (ObjectManager.Player.IsMoving)
            {
                ObjectManager.Player.StopAllMovement();
            }

            if (ObjectManager.Player.TargetGuid == ObjectManager.Player.Guid)
            {
                Functions.LuaCall($"SendChatMessage('.learn {spellId}')");
                BotTasks.Pop();
            }
            else
            {
                ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);
            }
        }
    }
}
