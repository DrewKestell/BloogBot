using RaidMemberBot.AI;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using System.Collections.Generic;

namespace RaidMemberBot.Tasks
{
    internal class ResetCharacterStateTask : BotTask, IBotTask
    {
        bool leaveParty;
        bool leaveBattleGround;
        bool resetLevel;
        bool resetSpells;
        bool resetTalents;
        bool attackMacroPickedUp;
        bool attackMacroPlaced;
        public ResetCharacterStateTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Ordinary) { }

        public void Update()
        {
            if (Wait.For("ResetStagger", 500))
                return;

            if (!leaveParty)
            {
                Functions.LuaCall("LeaveParty()");
                leaveParty = true;
            }
            else if (!resetLevel)
            {
                Functions.LuaCall($"SendChatMessage(\".character level {ObjectManager.Player.Name} 1\")");
                resetLevel = true;
            }
            else if (!resetSpells)
            {
                Functions.LuaCall($"SendChatMessage(\".reset spells {ObjectManager.Player.Name}\")");
                resetSpells = true;
            }
            else if (!resetTalents)
            {
                Functions.LuaCall($"SendChatMessage(\".reset talents {ObjectManager.Player.Name}\")");
                resetTalents = true;
            }
            else if (!attackMacroPickedUp)
            {
                Functions.LuaCall("PickupMacro(1)");
                attackMacroPickedUp = true;
            }
            else if (!attackMacroPlaced)
            {
                Functions.LuaCall("PlaceAction(72)");
                attackMacroPlaced = true;
            }


            if (resetLevel
                && !Container.State.InParty
                && resetSpells
                && resetTalents
                && attackMacroPickedUp
                && attackMacroPlaced)
            {
                Container.State.IsReset = true;
                BotTasks.Pop();
            }
        }
    }
}
