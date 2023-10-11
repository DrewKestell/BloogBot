using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace ProtectionPaladinBot
{
    class BuffTask : IBotTask
    {
        const string BlessingOfKings = "Blessing of Kings";
        const string BlessingOfMight = "Blessing of Might";
        const string BlessingOfSanctuary = "Blessing of Sanctuary";

        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if (!Spellbook.Instance.IsSpellReady(BlessingOfMight) || player.HasBuff(BlessingOfMight) || player.HasBuff(BlessingOfKings) || player.HasBuff(BlessingOfSanctuary))
            {
                botTasks.Pop();
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
            if (!player.HasBuff(name) && Spellbook.Instance.IsSpellReady(name) && player.Mana > player.GetManaCost(name))
            {
                Lua.Instance.Execute($"CastSpellByName(\"{name}\",1)");
            }
        }
    }
}
