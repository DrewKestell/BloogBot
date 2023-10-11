using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace CombatRogueBot
{
    class MoveToTargetTask : IBotTask
    {
        const string Distract = "Distract";
        const string Garrote = "Garrote";
        const string Stealth = "Stealth";
        const string CheapShot = "Cheap Shot";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.botTasks = botTasks;
            this.container = container;
            this.target = target;
            player = ObjectManager.Instance.Player;
            stuckHelper = new StuckHelper(container, botTasks);
        }

        public void Update()
        {
            if (target.TappedByOther)
            {
                player.StopAllMovement();
                botTasks.Pop();
                return;
            }

            if (!player.IsImmobilized)
                stuckHelper.CheckIfStuck();

            var distanceToTarget = player.Location.GetDistanceTo(target.Location);
            if (distanceToTarget < 25 && !player.HasBuff(Stealth) && Spellbook.Instance.IsSpellReady(Garrote) && !player.IsInCombat)
            {
                Lua.Instance.Execute($"CastSpellByName('{Stealth}')");
            }

            if (distanceToTarget < 15 && Spellbook.Instance.IsSpellReady(Distract) && Spellbook.Instance.IsSpellReady(Distract) && target.CreatureType != CreatureType.Totem)
            {
                //var delta = target.Location - player.Location;
                //var normalizedVector = delta.GetNormalizedVector();
                //var scaledVector = normalizedVector * 5;
                //var targetLocation = target.Location + scaledVector;

                //player.CastSpellAtPosition(Distract, targetPosition);
            }

            if (distanceToTarget < 3.5 && player.HasBuff(Stealth) && !player.IsInCombat && target.CreatureType != CreatureType.Totem)
            {
                if (Spellbook.Instance.IsSpellReady(Garrote) && target.CreatureType != CreatureType.Elemental && player.IsBehind(target))
                {
                    Lua.Instance.Execute($"CastSpellByName('{Garrote}')");
                    return;
                }
                else if (Spellbook.Instance.IsSpellReady(CheapShot) && player.IsBehind(target))
                {
                    Lua.Instance.Execute($"CastSpellByName('{CheapShot}')");
                    return;
                }
            } 

            if (distanceToTarget < 3)
            {
                player.StopAllMovement();
                botTasks.Pop();
                botTasks.Push(new PvERotationTask(container, botTasks));
                return;
            }

            var nextWaypoint = SocketClient.Instance.CalculatePath(player.MapId, player.Location, target.Location, false);
            player.MoveToward(nextWaypoint[0]);
        }
    }
}
