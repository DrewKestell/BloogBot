using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Tasks;

namespace PaladinProtection.Tasks 
{
    internal class BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Buff), IBotTask
    {
        private const string BlessingOfKings = "Blessing of Kings";
        private const string BlessingOfMight = "Blessing of Might";
        private const string BlessingOfSanctuary = "Blessing of Sanctuary";

        public void Update()
        {
            if (!ObjectManager.Player.IsSpellReady(BlessingOfMight) || ObjectManager.Player.HasBuff(BlessingOfMight) || ObjectManager.Player.HasBuff(BlessingOfKings) || ObjectManager.Player.HasBuff(BlessingOfSanctuary))
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsSpellReady(BlessingOfMight) && !ObjectManager.Player.IsSpellReady(BlessingOfKings) && !ObjectManager.Player.IsSpellReady(BlessingOfSanctuary))
                TryCastSpell(BlessingOfMight);

            if (ObjectManager.Player.IsSpellReady(BlessingOfKings) && !ObjectManager.Player.IsSpellReady(BlessingOfSanctuary))
                TryCastSpell(BlessingOfKings);

            if (ObjectManager.Player.IsSpellReady(BlessingOfSanctuary))
                TryCastSpell(BlessingOfSanctuary);
        }

        private void TryCastSpell(string name)
        {
            if (!ObjectManager.Player.HasBuff(name) && ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.Mana > ObjectManager.Player.GetManaCost(name))
            {
                Functions.LuaCall($"CastSpellByName(\"{name}\",1)");
            }
        }
    }
}
