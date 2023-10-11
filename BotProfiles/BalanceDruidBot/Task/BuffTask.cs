using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace BalanceDruidBot
{
    class BuffTask : IBotTask
    {
        const string MarkOfTheWild = "Mark of the Wild";
        const string Thorns = "Thorns";
        const string OmenOfClarity = "Omen of Clarity";
        const string MoonkinForm = "Moonkin Form";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if ((player.HasBuff(MarkOfTheWild) || !Spellbook.Instance.IsSpellReady(MarkOfTheWild)) &&
                (player.HasBuff(Thorns) || !Spellbook.Instance.IsSpellReady(Thorns)) &&
                (player.HasBuff(OmenOfClarity) || !Spellbook.Instance.IsSpellReady(OmenOfClarity)))
            {
                botTasks.Pop();
                return;
            }

            if (!player.HasBuff(MarkOfTheWild))
            {
                if (player.HasBuff(MoonkinForm))
                {
                    Lua.Instance.Execute($"CastSpellByName('{MoonkinForm}')");
                }

                TryCastSpell(MarkOfTheWild);
            }

            TryCastSpell(Thorns);
            TryCastSpell(OmenOfClarity);
        }

        void TryCastSpell(string name)
        {
            if (!player.HasBuff(name) && Spellbook.Instance.IsSpellReady(name))
                Lua.Instance.Execute($"CastSpellByName('{name}',1)");
        }
    }
}
