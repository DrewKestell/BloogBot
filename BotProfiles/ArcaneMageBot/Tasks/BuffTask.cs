using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace ArcaneMageBot
{
    class BuffTask : BotTask, IBotTask
    {
        const string ArcaneIntellect = "Arcane Intellect";
        const string FrostArmor = "Frost Armor";
        const string IceArmor = "Ice Armor";
        const string DampenMagic = "Dampen Magic";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
        public void Update()
        {
            if ((!Spellbook.Instance.IsSpellReady(ArcaneIntellect) || Container.Player.HasBuff(ArcaneIntellect)) && (Container.Player.HasBuff(FrostArmor) || Container.Player.HasBuff(IceArmor)) && (!Spellbook.Instance.IsSpellReady(DampenMagic) || Container.Player.HasBuff(DampenMagic)))
            {
                BotTasks.Pop();
                BotTasks.Push(new ConjureItemsTask(botTasks, container));
                return;
            }
            
            TryCastSpell(ArcaneIntellect, castOnSelf: true);

            if (Spellbook.Instance.IsSpellReady(IceArmor))
                TryCastSpell(IceArmor);
            else
                TryCastSpell(FrostArmor);

            TryCastSpell(DampenMagic, castOnSelf: true);
        }

        void TryCastSpell(string name, bool castOnSelf = false)
        {
            if (!Container.Player.HasBuff(name) && Spellbook.Instance.IsSpellReady(name) && Spellbook.Instance.IsSpellReady(name))
            {
                string castOnSelfString = castOnSelf ? ",1" : "";
                Lua.Instance.Execute($"CastSpellByName('{name}'{castOnSelfString})");
            }
        }
    }
}
