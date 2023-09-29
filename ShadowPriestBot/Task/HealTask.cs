using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace ShadowPriestBot
{
    class HealTask : IBotTask
    {
        const string LesserHeal = "Lesser Heal";
        const string Heal = "Heal";
        const string Renew = "Renew";

        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;

        readonly string healingSpell;

        public HealTask(Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            player = ObjectManager.Player;

            if (player.KnowsSpell(Heal))
                healingSpell = Heal;
            else
                healingSpell = LesserHeal;
        }

        public void Update()
        {
            if (player.IsCasting) return;

            if (player.HealthPercent > 70 || player.Mana < player.GetManaCost(healingSpell))
            {
                if (player.KnowsSpell(Renew) && player.Mana > player.GetManaCost(Renew))
                    player.LuaCall($"CastSpellByName('{Renew}',1)");

                botTasks.Pop();
                return;
            }

            player.LuaCall($"CastSpellByName('{healingSpell}',1)");
        }
    }
}
