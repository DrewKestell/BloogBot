using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
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
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(Container.HostileTarget.Position);
            if (distanceToTarget < 25 && !ObjectManager.Player.HasBuff(Stealth) && ObjectManager.Player.IsSpellReady(Garrote) && !ObjectManager.Player.IsInCombat)
            {
                Functions.LuaCall($"CastSpellByName('{Stealth}')");
            }

            if (distanceToTarget < 15 && ObjectManager.Player.IsSpellReady(Distract) && ObjectManager.Player.IsSpellReady(Distract) && Container.HostileTarget.CreatureType != CreatureType.Totem)
            {
                //var delta = Container.HostileTarget.Position - ObjectManager.Player.Position;
                //var normalizedVector = delta.GetNormalizedVector();
                //var scaledVector = normalizedVector * 5;
                //var targetPosition = Container.HostileTarget.Position + scaledVector;

                //ObjectManager.Player.CastSpellAtPosition(Distract, targetPosition);
            }

            if (distanceToTarget < 3.5 && ObjectManager.Player.HasBuff(Stealth) && !ObjectManager.Player.IsInCombat && Container.HostileTarget.CreatureType != CreatureType.Totem)
            {
                if (ObjectManager.Player.IsSpellReady(Garrote) && Container.HostileTarget.CreatureType != CreatureType.Elemental && ObjectManager.Player.IsBehind(Container.HostileTarget))
                {
                    Functions.LuaCall($"CastSpellByName('{Garrote}')");
                    return;
                }
                else if (ObjectManager.Player.IsSpellReady(CheapShot) && ObjectManager.Player.IsBehind(Container.HostileTarget))
                {
                    Functions.LuaCall($"CastSpellByName('{CheapShot}')");
                    return;
                }
            } 

            if (distanceToTarget < 3)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                BotTasks.Push(new PvERotationTask(Container, BotTasks));
                return;
            }

            Position[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, Container.HostileTarget.Position, true);
            ObjectManager.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
