using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace FeralDruidBot
{
    class HealTask : IBotTask
    {
        const string BearForm = "Bear Form";
        const string CatForm = "Cat Form";

        const string WarStomp = "War Stomp";
        const string HealingTouch = "Healing Touch";

        readonly Stack<IBotTask> botTasks;
        readonly WoWUnit target;
        readonly LocalPlayer player;

        public HealTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.botTasks = botTasks;
            this.target = target;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if (player.IsCasting) return;

            if (player.CurrentShapeshiftForm == BearForm && Wait.For("BearFormDelay", 1000, true))
                CastSpell(BearForm);

            if (player.CurrentShapeshiftForm == CatForm && Wait.For("CatFormDelay", 1000, true))
                CastSpell(CatForm);

            if (player.HealthPercent > 70 || player.Mana < player.GetManaCost(HealingTouch))
            {
                Wait.RemoveAll();
                botTasks.Pop();
                return;
            }

            if (Spellbook.Instance.IsSpellReady(WarStomp) && player.Location.GetDistanceTo(target.Location) <= 8)
                Lua.Instance.Execute($"CastSpellByName('{WarStomp}')");

            CastSpell(HealingTouch, castOnSelf: true);
        }

        void CastSpell(string name, bool castOnSelf = false)
        {
            if (Spellbook.Instance.IsSpellReady(name))
            {
                var castOnSelfString = castOnSelf ? ",1" : "";
                Lua.Instance.Execute($"CastSpellByName('{name}'{castOnSelfString})");
            }
        }
    }
}
