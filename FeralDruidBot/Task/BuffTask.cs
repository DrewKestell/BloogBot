using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace FeralDruidBot
{
    class BuffTask : IBotTask
    {
        const string MarkOfTheWild = "Mark of the Wild";
        const string Thorns = "Thorns";

        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> partyMembers)
        {
            this.botTasks = botTasks;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if ((player.HasBuff(MarkOfTheWild) || !player.KnowsSpell(MarkOfTheWild)) && (player.HasBuff(Thorns) || !player.KnowsSpell(Thorns)))
            {
                botTasks.Pop();
                return;
            }
            
            TryCastSpell(MarkOfTheWild);
            TryCastSpell(Thorns);
        }

        void TryCastSpell(string name)
        {
            if (!player.HasBuff(name) && player.KnowsSpell(name) && player.IsSpellReady(name))
                player.LuaCall($"CastSpellByName('{name}',1)");
        }
    }
}
