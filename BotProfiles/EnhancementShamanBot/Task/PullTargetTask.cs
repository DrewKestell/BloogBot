using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace EnhancementShamanBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string LightningBolt = "Lightning Bolt";
        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull) { }

        public void Update()
        {
            if (Container.HostileTarget.TappedByOther || (ObjectManager.Aggressors.Count() > 0 && !ObjectManager.Aggressors.Any(a => a.Guid == Container.HostileTarget.Guid)))
            {
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Position.DistanceTo(Container.HostileTarget.Position) < 27 && ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(LightningBolt) && ObjectManager.Player.InLosWith(Container.HostileTarget.Position))
            {
                if (ObjectManager.Player.IsMoving)
                    ObjectManager.Player.StopAllMovement();

                if (Wait.For("PullWithLightningBoltDelay", 100))
                {
                    if (!ObjectManager.Player.IsInCombat)
                        Functions.LuaCall($"CastSpellByName('{LightningBolt}')");

                    if (ObjectManager.Player.IsCasting || ObjectManager.Player.IsInCombat)
                    {
                        ObjectManager.Player.StopAllMovement();
                        Wait.RemoveAll();
                        BotTasks.Pop();
                        BotTasks.Push(new PvERotationTask(Container, BotTasks));
                    }
                }
                return;
            }

            Position[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, Container.HostileTarget.Position, true);
            ObjectManager.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
