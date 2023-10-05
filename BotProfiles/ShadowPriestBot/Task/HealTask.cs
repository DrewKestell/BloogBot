using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
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
            player = ObjectManager.Instance.Player;

            if (Spellbook.Instance.IsSpellReady(Heal))
                healingSpell = Heal;
            else
                healingSpell = LesserHeal;
        }

        public void Update()
        {
            if (player.IsCasting) return;

            if (player.HealthPercent > 70 || player.Mana < player.GetManaCost(healingSpell))
            {
                if (Spellbook.Instance.IsSpellReady(Renew) && player.Mana > player.GetManaCost(Renew))
                    Lua.Instance.Execute($"CastSpellByName('{Renew}',1)");

                botTasks.Pop();
                return;
            }

            Lua.Instance.Execute($"CastSpellByName('{healingSpell}',1)");
        }
    }
}
