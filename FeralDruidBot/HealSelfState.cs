using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;

namespace FeralDruidBot
{
    class HealSelfState : IBotState
    {
        const string WarStomp = "War Stomp";
        const string HealingTouch = "Healing Touch";
        const string SurvivalInstincts = "Survival Instincts";
        const string Barkskin = "Barkskin";

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

            player.StopAllMovement();

            if (player.HealthPercent > 70 || player.Mana < player.GetManaCost(HealingTouch))
            {
                Wait.RemoveAll();
                botStates.Pop();
                return;
            }

            CastSpell(SurvivalInstincts);

            if (!player.HasBuff(SurvivalInstincts))
            {
                CastSpell(Barkskin);
            }

            if (player.IsSpellReady(WarStomp) && player.Position.DistanceTo(target.Position) <= 8)
                player.LuaCall($"CastSpellByName('{WarStomp}')");

            CastSpell(HealingTouch, castOnSelf: true);
        }

        void CastSpell(string name, bool castOnSelf = false)
        {
            if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
            {
                var castOnSelfString = castOnSelf ? ",1" : "";
                player.LuaCall($"CastSpellByName(\"{name}\"{castOnSelfString})");
            }
            else
            {
                var targetGuid = castOnSelf ? player.Guid : target.Guid;
                player.CastSpell(name, targetGuid);
            }
        }
    }
}
