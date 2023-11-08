using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
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
            if (Container.HostileTarget.TappedByOther || (ObjectManager.Instance.Aggressors.Count() > 0 && !ObjectManager.Instance.Aggressors.Any(a => a.Guid == Container.HostileTarget.Guid)))
            {
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (Container.Player.Location.GetDistanceTo(Container.HostileTarget.Location) < 27 && Container.Player.IsCasting && Spellbook.Instance.IsSpellReady(LightningBolt) && Container.Player.InLosWith(Container.HostileTarget.Location))
            {
                if (Container.Player.IsMoving)
                    Container.Player.StopAllMovement();

                if (Wait.For("PullWithLightningBoltDelay", 100))
                {
                    if (!Container.Player.IsInCombat)
                        Lua.Instance.Execute($"CastSpellByName('{LightningBolt}')");

                    if (Container.Player.IsCasting || Container.Player.IsInCombat)
                    {
                        Container.Player.StopAllMovement();
                        Wait.RemoveAll();
                        BotTasks.Pop();
                        BotTasks.Push(new PvERotationTask(Container, BotTasks));
                    }
                }
                return;
            }

            Location[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);
            Container.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
