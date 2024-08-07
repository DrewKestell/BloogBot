using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    public class AddSpellTask(IBotContext botContext, int spellId) : BotTask(botContext), IBotTask
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
                ObjectManager.SendChatMessage($".learn {spellId}");
                BotTasks.Pop();
            }
            else
            {
                ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);
            }
        }
    }
}
