using System;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;

namespace BloogBot.AI.SharedStates
{
    public abstract class SpellCastingStateBase
    {
        readonly LocalPlayer player;
        readonly WoWUnit target;

        protected SpellCastingStateBase(LocalPlayer player, WoWUnit target)
        {
            this.player = player;
            this.target = target;
        }

        public void TryCastSpell(
            string name,
            int minRange,
            int maxRange,
            bool condition = true,
            Action callback = null,
            bool castOnSelf = false)
        {
            TryCastSpellInternal(name, minRange, maxRange, condition, callback, castOnSelf);
        }

        public void TryCastSpell(
            string name,
            bool condition = true,
            Action callback = null,
            bool castOnSelf = false)
        {
            TryCastSpellInternal(name, 0, int.MaxValue, condition, callback, castOnSelf);
        }

        void TryCastSpellInternal(
            string name,
            int minRange,
            int maxRange,
            bool condition = true,
            Action callback = null,
            bool castOnSelf = false)
        {
            var distanceToTarget = player.Position.DistanceTo(target.Position);

            if (player.IsSpellReady(name) &&
                player.Mana >= player.GetManaCost(name) &&
                distanceToTarget >= minRange &&
                distanceToTarget <= maxRange &&
                condition &&
                !player.IsStunned &&
                ((!player.IsCasting && !player.IsChanneling) || player.Class == Class.Warrior))
            {
                if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
                {
                    var castOnSelfString = castOnSelf ? ",1" : "";
                    player.LuaCall($"CastSpellByName(\"{name}\"{castOnSelfString})");
                    callback?.Invoke();
                }
                else
                {
                    var targetGuid = castOnSelf ? player.Guid : target.Guid;
                    player.CastSpell(name, targetGuid);
                    callback?.Invoke();
                }
            }
        }

    }
}