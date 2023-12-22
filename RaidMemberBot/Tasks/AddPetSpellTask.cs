using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
{
    public class AddPetSpellTask : BotTask, IBotTask
    {
        int spellId;
        public AddPetSpellTask(IClassContainer container, Stack<IBotTask> botTasks, int spellId) : base(container, botTasks, TaskType.Ordinary) {
            this.spellId = spellId;
        }
        public void Update()
        {
            if (ObjectManager.Player.IsMoving)
            {
                ObjectManager.Player.StopAllMovement();
            }

            if (ObjectManager.Player.TargetGuid == ObjectManager.Player.Guid)
            {
                Functions.LuaCall($"SendChatMessage('.learn {spellId}')");
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
