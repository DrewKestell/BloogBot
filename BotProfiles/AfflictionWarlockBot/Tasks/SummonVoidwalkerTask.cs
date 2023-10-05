using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
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
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if (player.Casting > 0)
                return;

            
            if ((!Spellbook.Instance.IsSpellReady(SummonImp) && !Spellbook.Instance.IsSpellReady(SummonVoidwalker)) || ObjectManager.Instance.Pet != null)
            {
                botTasks.Pop();
                botTasks.Push(new BuffTask(container, botTasks, new List<WoWUnit>()));
                return;
            }

            if (Spellbook.Instance.IsSpellReady(SummonVoidwalker))
                Lua.Instance.Execute($"CastSpellByName('{SummonVoidwalker}')");
            else
                Lua.Instance.Execute($"CastSpellByName('{SummonImp}')");
        }
    }
}
