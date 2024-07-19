using WoWSlimClient.Models;

namespace WoWSlimClient.Tasks
{
    internal class ResetActivityMemberStateTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private bool startedTeleport;
        private bool leaveParty;
        private bool leaveBattleGround;
        private bool resetLevel;
        private bool resetSpells;
        private bool resetTalents;
        private bool resetInstances;
        private bool attackMacroPickedUp;
        private bool attackMacroPlaced;

        public void Update()
        {
            if (Wait.For("ResetStagger", 1000))
                return;

            if (!startedTeleport)
            {
                //Functions.LuaCall($"SendChatMessage(\".go xyz 16226 16257 13 1\")");
                startedTeleport = true;
            }
            else if (!leaveParty)
            {
                //Functions.LuaCall("LeaveParty()");
                leaveParty = true;
            }
            else if (!resetLevel)
            {
                //Functions.LuaCall($"SendChatMessage(\".character level {ObjectManager.Instance.Player.Name} 1\")");
                resetLevel = true;
            }
            else if (!resetSpells)
            {
                //Functions.LuaCall($"SendChatMessage(\".reset spells {ObjectManager.Instance.Player.Name}\")");
                resetSpells = true;
            }
            else if (!resetTalents)
            {
                //Functions.LuaCall($"SendChatMessage(\".reset talents {ObjectManager.Instance.Player.Name}\")");
                resetTalents = true;
            }
            else if (!resetInstances)
            {
                //Functions.LuaCall("ResetInstances()");
                resetInstances = true;
            }
            else if (!attackMacroPickedUp)
            {
                //Functions.LuaCall("PickupMacro(1)");
                attackMacroPickedUp = true;
            }
            else if (!attackMacroPlaced)
            {
                //Functions.LuaCall("PlaceAction(72)");
                attackMacroPlaced = true;
            }


            if (resetLevel
                && resetSpells
                && resetTalents
                && attackMacroPickedUp
                && attackMacroPlaced)
            {
                BotTasks.Pop();
            }
        }
    }
}
