using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace CombatRogueBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string Distract = "Distract";
        const string Garrote = "Garrote";
        const string Stealth = "Stealth";
        const string CheapShot = "Cheap Shot";

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull) { }

        public void Update()
        {
            if (Container.HostileTarget.TappedByOther)
            {
                Container.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            float distanceToTarget = Container.Player.Location.GetDistanceTo(Container.HostileTarget.Location);
            if (distanceToTarget < 25 && !Container.Player.HasBuff(Stealth) && Spellbook.Instance.IsSpellReady(Garrote) && !Container.Player.IsInCombat)
            {
                Lua.Instance.Execute($"CastSpellByName('{Stealth}')");
            }

            if (distanceToTarget < 15 && Spellbook.Instance.IsSpellReady(Distract) && Spellbook.Instance.IsSpellReady(Distract) && Container.HostileTarget.CreatureType != CreatureType.Totem)
            {
                //var delta = Container.HostileTarget.Location - Container.Player.Location;
                //var normalizedVector = delta.GetNormalizedVector();
                //var scaledVector = normalizedVector * 5;
                //var targetLocation = Container.HostileTarget.Location + scaledVector;

                //Container.Player.CastSpellAtPosition(Distract, targetPosition);
            }

            if (distanceToTarget < 3.5 && Container.Player.HasBuff(Stealth) && !Container.Player.IsInCombat && Container.HostileTarget.CreatureType != CreatureType.Totem)
            {
                if (Spellbook.Instance.IsSpellReady(Garrote) && Container.HostileTarget.CreatureType != CreatureType.Elemental && Container.Player.IsBehind(Container.HostileTarget))
                {
                    Lua.Instance.Execute($"CastSpellByName('{Garrote}')");
                    return;
                }
                else if (Spellbook.Instance.IsSpellReady(CheapShot) && Container.Player.IsBehind(Container.HostileTarget))
                {
                    Lua.Instance.Execute($"CastSpellByName('{CheapShot}')");
                    return;
                }
            } 

            if (distanceToTarget < 3)
            {
                Container.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(Container, BotTasks));
                return;
            }

            RaidMemberBot.Objects.Location[] nextWaypoint = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);
            Container.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
