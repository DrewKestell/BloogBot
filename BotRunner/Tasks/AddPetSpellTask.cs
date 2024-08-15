using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    public class AddPetSpellTask(IBotContext botContext, uint spellId) : BotTask(botContext), IBotTask
    {
        private readonly uint spellId = spellId;

        public void Update()
        {
            if (ObjectManager.Player.IsMoving)
            {
                ObjectManager.Player.StopAllMovement();
            }

            if (ObjectManager.GetTarget(ObjectManager.Player).Guid == ObjectManager.Player.Guid)
            {
                ObjectManager.SendChatMessage($".learn {spellId}");
                Container.State.PetSpells.Add(spellId);
                BotTasks.Pop();
            }
            else
            {
                ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);
            }
        }
    }
}
