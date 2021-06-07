using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;

namespace BalanceDruidBot
{
    class PowerlevelCombatState : IBotState
    {
        const string LosErrorMessage = "Target not in line of sight";

        const string AutoAttackLuaScript = "if IsCurrentAction('12') == nil then CastSpellByName('Attack') end";

        const string HealingTouch = "Healing Touch";
        const string MarkOfTheWild = "Mark of the Wild";
        const string Moonfire = "Moonfire";
        const string Regrowth = "Regrowth";
        const string Rejuvenation = "Rejuvenation";
        const string Thorns = "Thorns";
        const string Wrath = "Wrath";

        const int spellRange = 29;

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly WoWUnit target;
        readonly WoWPlayer powerlevelTarget;
        readonly LocalPlayer player;

        bool noLos;
        int noLosStartTime;

        public PowerlevelCombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target, WoWPlayer powerlevelTarget)
        {
            this.botStates = botStates;
            this.container = container;
            this.target = target;
            this.powerlevelTarget = powerlevelTarget;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            // handle no los with target
            if (Environment.TickCount - noLosStartTime > 1000)
            {
                player.StopAllMovement();
                noLos = false;
            }
            if (noLos)
            {
                var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
                player.MoveToward(nextWaypoint);
                return;
            }

            // heal self if we're injured
            if (player.HealthPercent < 30 && (player.Mana >= player.GetManaCost(HealingTouch) || player.Mana >= player.GetManaCost(Rejuvenation)))
            {
                botStates.Push(new HealSelfState(botStates, target));
                return;
            }
            
            // pop state when the target is dead
            if (target.Health == 0)
            {
                botStates.Pop();

                if (player.ManaPercent < 20)
                    botStates.Push(new RestState(botStates, container));

                return;
            }

            // make sure the target is set (sometimes it inexplicably gets unset)
            if (player.TargetGuid != target.Guid)
                player.SetTarget(target.Guid);

            // ensure we're facing the target
            if (!player.IsFacing(target.Position)) player.Face(target.Position);

            // ensure auto-attack is turned on
            player.LuaCall(AutoAttackLuaScript);

            // ensure we're in casting range, or melee range if oom
            if (player.Position.DistanceTo(target.Position) > 29 || (player.Mana < player.GetManaCost(Wrath) && player.Position.DistanceTo(target.Position) > 4))
            {
                var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
                player.MoveToward(nextWaypoint);
            }
            else
                player.StopAllMovement();

            // combat rotation
            TryCastSpell(Regrowth, 0, 29, powerlevelTarget.HealthPercent < 40);

            if (!powerlevelTarget.HasBuff(MarkOfTheWild))
            {
                player.SetTarget(powerlevelTarget.Guid);
                TryCastSpell(MarkOfTheWild, 0, 29);
            }
            
            if (!powerlevelTarget.HasBuff(Thorns))
            {
                player.SetTarget(powerlevelTarget.Guid);
                TryCastSpell(Thorns, 0, 29);
            }

            TryCastSpell(Moonfire, 0, 29, !target.HasDebuff(Moonfire));

            TryCastSpell(Wrath, 0, spellRange);
        }

        void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false)
        {
            var distanceToTarget = player.Position.DistanceTo(target.Position);

            if (player.IsSpellReady(name) && player.Mana >= player.GetManaCost(name) && distanceToTarget >= minRange && distanceToTarget <= maxRange && condition && !player.IsStunned && !player.IsCasting && !player.IsChanneling)
            {
                var castOnSelfString = castOnSelf ? ",1" : "";
                player.LuaCall($"CastSpellByName(\"{name}\"{castOnSelfString})");
                callback?.Invoke();
            }
        }

        void OnErrorMessageCallback(object sender, OnUiMessageArgs e)
        {
            if (e.Message == LosErrorMessage)
            {
                noLos = true;
                noLosStartTime = Environment.TickCount;
            }
        }
    }
}
