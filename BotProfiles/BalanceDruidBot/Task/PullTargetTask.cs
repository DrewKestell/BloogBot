using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BalanceDruidBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string Wrath = "Wrath";
        const string Starfire = "Starfire";
        const string MoonkinForm = "Moonkin Form";

        readonly int range;
        readonly string pullingSpell;

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull)
        {
            if (Container.Player.Level <= 19)
                range = 28;
            else if (Container.Player.Level == 20)
                range = 31;
            else
                range = 34;

            if (Spellbook.Instance.IsSpellReady(Starfire))
                pullingSpell = Starfire;
            else
                pullingSpell = Wrath;
        }

        public void Update()
        {
            if (Container.HostileTarget.TappedByOther || (ObjectManager.Instance.Aggressors.Count() > 0 && !ObjectManager.Instance.Aggressors.Any(a => a.Guid == Container.HostileTarget.Guid)))
            {
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (Container.Player.IsCasting)
                return;

            if (Spellbook.Instance.IsSpellReady(MoonkinForm) && !Container.Player.HasBuff(MoonkinForm))
            {
                Lua.Instance.Execute($"CastSpellByName('{MoonkinForm}')");
            }

            if (Container.Player.Location.GetDistanceTo(Container.HostileTarget.Location) < range && Container.Player.IsCasting && Spellbook.Instance.IsSpellReady(pullingSpell) && Container.Player.InLosWith(Container.HostileTarget.Location))
            {
                if (Container.Player.IsMoving)
                    Container.Player.StopAllMovement();

                if (Wait.For("BalanceDruidPullDelay", 100))
                {
                    if (!Container.Player.IsInCombat)
                        Lua.Instance.Execute($"CastSpellByName('{pullingSpell}')");

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

            Location[] nextWaypoint = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);
            Container.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
