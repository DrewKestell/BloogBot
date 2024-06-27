using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Tasks;

namespace PaladinProtection.Tasks 
{
    class BuffTask : BotTask, IBotTask
    {
        const string BlessingOfKings = "Blessing of Kings";
        const string BlessingOfMight = "Blessing of Might";
        const string BlessingOfSanctuary = "Blessing of Sanctuary";

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
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

        void TryCastSpell(string name)
        {
            if (!ObjectManager.Player.HasBuff(name) && ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.Mana > ObjectManager.Player.GetManaCost(name))
            {
                Functions.LuaCall($"CastSpellByName(\"{name}\",1)");
            }
        }
    }
}
