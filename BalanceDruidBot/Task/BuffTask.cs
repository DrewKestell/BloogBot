using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
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
        readonly LocalPlayer player;

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> partyMembers)
        {
            this.botTasks = botTasks;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if ((player.HasBuff(MarkOfTheWild) || !player.KnowsSpell(MarkOfTheWild)) &&
                (player.HasBuff(Thorns) || !player.KnowsSpell(Thorns)) &&
                (player.HasBuff(OmenOfClarity) || !player.KnowsSpell(OmenOfClarity)))
            {
                botTasks.Pop();
                return;
            }

            if (!player.HasBuff(MarkOfTheWild))
            {
                if (player.HasBuff(MoonkinForm))
                {
                    player.LuaCall($"CastSpellByName('{MoonkinForm}')");
                }

                TryCastSpell(MarkOfTheWild);
            }

            TryCastSpell(Thorns);
            TryCastSpell(OmenOfClarity);
        }

        void TryCastSpell(string name)
        {
            if (!player.HasBuff(name) && player.IsSpellReady(name))
                player.LuaCall($"CastSpellByName('{name}',1)");
        }
    }
}
