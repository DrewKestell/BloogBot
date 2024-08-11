using BotRunner.Interfaces;

namespace BotRunner.Tasks
{
    internal class ResetActivityMemberStateTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        private bool startedTeleport;
        private bool leaveParty;
        private readonly bool leaveBattleGround;
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
                ObjectManager.SendChatMessage(".go xyz 16226 16257 13 1");
                startedTeleport = true;
            }
            else if (!leaveParty)
            {
                ObjectManager.LeaveGroup();
                leaveParty = true;
            }
            else if (!resetLevel)
            {
                ObjectManager.SendChatMessage($".character level {ObjectManager.Player.Name} 1");
                resetLevel = true;
            }
            else if (!resetSpells)
            {
                ObjectManager.SendChatMessage($".reset spells {ObjectManager.Player.Name}");
                resetSpells = true;
            }
            else if (!resetTalents)
            {
                ObjectManager.SendChatMessage($".reset talents {ObjectManager.Player.Name}");
                resetTalents = true;
            }
            else if (!resetInstances)
            {
                ObjectManager.ResetInstances();
                resetInstances = true;
            }
            else if (!attackMacroPickedUp)
            {
                ObjectManager.PickupMacro(1);
                attackMacroPickedUp = true;
            }
            else if (!attackMacroPlaced)
            {
                ObjectManager.PlaceAction(72);
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
