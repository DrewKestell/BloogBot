using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace FeralDruidBot
{
    class HealSelfState : IBotState
    {
        const string BearForm = "Bear Form";
        const string CatForm = "Cat Form";

        const string WarStomp = "War Stomp";
        const string HealingTouch = "Healing Touch";

        readonly Stack<IBotState> botStates;
        readonly WoWUnit target;
        readonly LocalPlayer player;

        public HealSelfState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target)
        {
            this.botStates = botStates;
            this.target = target;
            player = ObjectManager.Player;
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
                botStates.Pop();
                return;
            }

            if (player.IsSpellReady(WarStomp) && player.Position.DistanceTo(target.Position) <= 8)
                player.LuaCall($"CastSpellByName('{WarStomp}')");

            CastSpell(HealingTouch, castOnSelf: true);
        }

        void CastSpell(string name, bool castOnSelf = false)
        {
            if (player.IsSpellReady(name))
            {
                var castOnSelfString = castOnSelf ? ",1" : "";
                player.LuaCall($"CastSpellByName('{name}'{castOnSelfString})");
            }
        }
    }
}
