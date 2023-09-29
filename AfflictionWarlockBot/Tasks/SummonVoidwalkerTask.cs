using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace AfflictionWarlockBot
{
    class SummonVoidwalkerTask : IBotTask
    {
        const string SummonImp = "Summon Imp";
        const string SummonVoidwalker = "Summon Voidwalker";
        
        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;

        public SummonVoidwalkerTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.container = container;
            this.botTasks = botTasks;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if (player.IsCasting)
                return;

            
            if ((!player.KnowsSpell(SummonImp) && !player.KnowsSpell(SummonVoidwalker)) || ObjectManager.Pet != null)
            {
                botTasks.Pop();
                botTasks.Push(new BuffTask(container, botTasks, new List<WoWUnit>()));
                return;
            }

            if (player.KnowsSpell(SummonVoidwalker))
                player.LuaCall($"CastSpellByName('{SummonVoidwalker}')");
            else
                player.LuaCall($"CastSpellByName('{SummonImp}')");
        }
    }
}
