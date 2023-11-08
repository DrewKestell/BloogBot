using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace ArcaneMageBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string Fireball = "Fireball";
        const string Frostbolt = "Frostbolt";

        readonly string pullingSpell;

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull)
        {
            if (Spellbook.Instance.IsSpellReady(Frostbolt))
                pullingSpell = Frostbolt;
            else
                pullingSpell = Fireball;
        }

        public void Update()
        {
            if (Container.HostileTarget.TappedByOther)
            {
                Container.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            float distanceToTarget = Container.Player.Location.GetDistanceTo(Container.HostileTarget.Location);
            if (distanceToTarget < 27)
            {
                if (Container.Player.IsMoving)
                    Container.Player.StopAllMovement();

                if (Container.Player.IsCasting && Spellbook.Instance.IsSpellReady(pullingSpell) && Wait.For("ArcaneMagePull", 500))
                {
                    Container.Player.StopAllMovement();
                    Wait.RemoveAll();
                    Lua.Instance.Execute($"CastSpellByName('{pullingSpell}')");
                    BotTasks.Pop();
                    BotTasks.Push(new PvERotationTask(Container, BotTasks));
                    return;
                }
            }
            else
            {
                Location[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);
                Container.Player.MoveToward(nextWaypoint[0]);
            }
        }
    }
}
