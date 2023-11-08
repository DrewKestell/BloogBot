using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace FeralDruidBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string Wrath = "Wrath";

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull) { }
        public void Update()
        {
            if (Container.HostileTarget.TappedByOther || (ObjectManager.Instance.Aggressors.Count() > 0 && !ObjectManager.Instance.Aggressors.Any(a => a.Guid == Container.HostileTarget.Guid)))
            {
                Container.Player.StopAllMovement();
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }
            
            if (Container.Player.Location.GetDistanceTo(Container.HostileTarget.Location) < 27 && Container.Player.IsCasting && Spellbook.Instance.IsSpellReady(Wrath) && Container.Player.InLosWith(Container.HostileTarget.Location))
            {
                if (Container.Player.IsMoving)
                    Container.Player.StopAllMovement();

                if (Wait.For("PullWithWrathDelay", 100))
                {
                    if (!Container.Player.IsInCombat)
                        Lua.Instance.Execute($"CastSpellByName('{Wrath}')");

                    if (Container.Player.IsCasting || Container.Player.CurrentShapeshiftForm != "Human Form" || Container.Player.IsInCombat)
                    {
                        Container.Player.StopAllMovement();
                        Wait.RemoveAll();
                        BotTasks.Pop();
                        BotTasks.Push(new PvERotationTask(Container, BotTasks));
                    }
                }
                return;
            }

            Location[] nextWaypoint = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);
            Container.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
