using WoWClientBot.AI;
using WoWClientBot.Game;
using WoWClientBot.Game.Statics;
using WoWClientBot.Mem;

namespace WoWClientBot.Tasks
{
    internal class ResetCharacterStateTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        bool leaveParty;
        readonly bool leaveBattleGround;
        bool resetLevel;
        bool resetSpells;
        bool resetTalents;
        bool attackMacroPickedUp;
        bool attackMacroPlaced;

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
