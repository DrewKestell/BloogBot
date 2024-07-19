using WoWSlimClient.Manager;

namespace WoWSlimClient.Tasks.SharedStates
{
    public class AddPetSpellTask(IClassContainer container, Stack<IBotTask> botTasks, int spellId) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private readonly int spellId = spellId;

        public void Update()
        {
            if (ObjectManager.Instance.Player.IsMoving)
            {
                ObjectManager.Instance.Player.StopAllMovement();
            }

            if (ObjectManager.Instance.Player.TargetGuid == ObjectManager.Instance.Player.Guid)
            {                
                BotTasks.Pop();
            }
            else
            {
                ObjectManager.Instance.Player.SetTarget(ObjectManager.Instance.Player.Guid);
            }
        }
    }
}
