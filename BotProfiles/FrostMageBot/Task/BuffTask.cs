using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using System.Collections.Generic;

namespace FrostMageBot
{
    class BuffTask : BotTask, IBotTask
    {
        const string ArcaneIntellect = "Arcane Intellect";
        const string DampenMagic = "Dampen Magic";
        const string FrostArmor = "Frost Armor";
        const string IceArmor = "Ice Armor";
        const string MageArmor = "Mage Armor";

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
        public void Update()
        {
            if ((!Spellbook.Instance.IsSpellReady(ArcaneIntellect) || Container.Player.HasBuff(ArcaneIntellect)) && (Container.Player.HasBuff(FrostArmor) || Container.Player.HasBuff(IceArmor) || Container.Player.HasBuff(MageArmor)) && (!Spellbook.Instance.IsSpellReady(DampenMagic) || Container.Player.HasBuff(DampenMagic)))
            {
                BotTasks.Pop();
                BotTasks.Push(new ConjureItemsTask(BotTasks, Container));
                return;
            }

            TryCastSpell(ArcaneIntellect, castOnSelf: true);

            if (Spellbook.Instance.IsSpellReady(MageArmor))
                TryCastSpell(MageArmor);
            else if (Spellbook.Instance.IsSpellReady(IceArmor))
                TryCastSpell(IceArmor);
            else
                TryCastSpell(FrostArmor);

            TryCastSpell(DampenMagic, castOnSelf: true);
        }

        void TryCastSpell(string name, bool castOnSelf = false)
        {
            if (!Container.Player.HasBuff(name) && Spellbook.Instance.IsSpellReady(name) && Spellbook.Instance.IsSpellReady(name))
            {
                if (castOnSelf)
                {
                    Lua.Instance.Execute($"CastSpellByName(\"{name}\",1)");
                }
                else
                    Lua.Instance.Execute($"CastSpellByName('{name}')");
            }
        }
    }
}
