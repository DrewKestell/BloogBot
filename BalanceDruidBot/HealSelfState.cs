using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace BalanceDruidBot
{
    class HealSelfState : IBotState
    {
        const string WarStomp = "War Stomp";
        const string HealingTouch = "Healing Touch";
        const string Rejuvenation = "Rejuvenation";
        const string Barkskin = "Barkskin";
        const string MoonkinForm = "Moonkin Form";

        readonly Stack<IBotState> botStates;
        readonly WoWUnit target;
        readonly LocalPlayer player;

        public HealSelfState(Stack<IBotState> botStates, WoWUnit target)
        {
            this.botStates = botStates;
            this.target = target;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if (player.IsCasting) return;

            if (player.HealthPercent > 70 || (player.Mana < player.GetManaCost(HealingTouch) && player.Mana < player.GetManaCost(Rejuvenation)))
            {
                Wait.RemoveAll();
                botStates.Pop();
                return;
            }

            if (player.IsSpellReady(WarStomp) && player.Position.DistanceTo(target.Position) <= 8)
                player.LuaCall($"CastSpellByName('{WarStomp}')");

            TryCastSpell(MoonkinForm, player.HasBuff(MoonkinForm));

            TryCastSpell(Barkskin);

            TryCastSpell(Rejuvenation, !player.HasBuff(Rejuvenation));

            TryCastSpell(HealingTouch);
        }

        void TryCastSpell(string name, bool condition = true)
        {
            if (player.IsSpellReady(name) && condition)
                player.LuaCall($"CastSpellByName('{name}',1)");
        }
    }
}
