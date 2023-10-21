using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using System.Collections.Generic;

namespace FrostMageBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string waitKey = "FrostMagePull";

        const string Fireball = "Fireball";
        const string Frostbolt = "Frostbolt";

        readonly string pullingSpell;
        readonly int range;

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull)
        {
            if (Spellbook.Instance.IsSpellReady(Frostbolt))
                pullingSpell = Frostbolt;
            else
                pullingSpell = Fireball;

            range = 28 + (Container.Player.GetTalentRank(3, 11) * 3);
        }

        public void Update()
        {
            if (Container.Player.IsCasting)
                return;

            if (Container.HostileTarget.TappedByOther)
            {
                Container.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            var distanceToTarget = Container.Player.Location.GetDistanceTo(Container.HostileTarget.Location);
            if (distanceToTarget <= range && Container.Player.InLosWith(Container.HostileTarget.Location))
            {
                if (Container.Player.IsMoving)
                    Container.Player.StopAllMovement();

                if (Wait.For(waitKey, 250))
                {
                    Container.Player.StopAllMovement();
                    Wait.Remove(waitKey);
                    
                    if (!Container.Player.IsInCombat)
                        Lua.Instance.Execute($"CastSpellByName('{pullingSpell}')");

                    BotTasks.Pop();
                    BotTasks.Push(new PvERotationTask(Container, BotTasks));
                    return;
                }
            }
            else
            {
                var nextWaypoint = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);
                Container.Player.MoveToward(nextWaypoint[0]);
            }
        }
    }
}
