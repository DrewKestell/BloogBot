using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using System.Collections.Generic;

namespace RetributionPaladinBot
{
    class BuffTask : BotTask, IBotTask
    {
        const string BlessingOfKings = "Blessing of Kings";
        const string BlessingOfMight = "Blessing of Might";
        const string BlessingOfSanctuary = "Blessing of Sanctuary";
        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
        public void Update()
        {
            if (!Spellbook.Instance.IsSpellReady(BlessingOfMight) || Container.Player.HasBuff(BlessingOfMight) || Container.Player.HasBuff(BlessingOfKings) || Container.Player.HasBuff(BlessingOfSanctuary))
            {
                BotTasks.Pop();
                return;
            }
            
            if (Spellbook.Instance.IsSpellReady(BlessingOfMight) && !Spellbook.Instance.IsSpellReady(BlessingOfKings) && !Spellbook.Instance.IsSpellReady(BlessingOfSanctuary))
                TryCastSpell(BlessingOfMight);

            if (Spellbook.Instance.IsSpellReady(BlessingOfKings) && !Spellbook.Instance.IsSpellReady(BlessingOfSanctuary))
                TryCastSpell(BlessingOfKings);
            
            if (Spellbook.Instance.IsSpellReady(BlessingOfSanctuary))
                TryCastSpell(BlessingOfSanctuary);
        }

        void TryCastSpell(string name)
        {
            if (!Container.Player.HasBuff(name) && Spellbook.Instance.IsSpellReady(name) && Container.Player.Mana > Container.Player.GetManaCost(name))
                Lua.Instance.Execute($"CastSpellByName('{name}',1)");
        }
    }
}
