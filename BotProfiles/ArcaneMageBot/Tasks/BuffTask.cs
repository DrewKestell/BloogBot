using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace ArcaneMageBot
{
    class BuffTask : IBotTask
    {
        const string ArcaneIntellect = "Arcane Intellect";
        const string FrostArmor = "Frost Armor";
        const string IceArmor = "Ice Armor";
        const string DampenMagic = "Dampen Magic";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> partyMembers)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if ((!Spellbook.Instance.IsSpellReady(ArcaneIntellect) || player.HasBuff(ArcaneIntellect)) && (player.HasBuff(FrostArmor) || player.HasBuff(IceArmor)) && (!Spellbook.Instance.IsSpellReady(DampenMagic) || player.HasBuff(DampenMagic)))
            {
                botTasks.Pop();
                botTasks.Push(new ConjureItemsTask(botTasks, container));
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
            if (!player.HasBuff(name) && Spellbook.Instance.IsSpellReady(name) && Spellbook.Instance.IsSpellReady(name))
            {
                var castOnSelfString = castOnSelf ? ",1" : "";
                Lua.Instance.Execute($"CastSpellByName('{name}'{castOnSelfString})");
            }
        }
    }
}
