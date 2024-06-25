using WoWClientBot.Game.Statics;
using WoWClientBot.Mem;

namespace WoWClientBot.AI.SharedStates
{
    public class AddTalentTask(IClassContainer container, Stack<IBotTask> botTasks, int spellId) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        readonly int spellId = spellId;

        public void Update()
        {
            if (ObjectManager.Player.IsMoving)
            {
                ObjectManager.Player.StopAllMovement();
            }
            if (ObjectManager.Player.TargetGuid == ObjectManager.Player.Guid)
            {
                Functions.LuaCall($"SendChatMessage('.learn {spellId}')");
                Container.State.CharacterConfig.Talents.Add(spellId);
                BotTasks.Pop();
            }
            else
            {
                ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);
            }
        }
    }
}
