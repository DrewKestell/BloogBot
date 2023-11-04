using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace AfflictionWarlockBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string SummonImp = "Summon Imp";
        const string SummonVoidwalker = "Summon Voidwalker";
        const string CurseOfAgony = "Curse of Agony";
        const string ShadowBolt = "Shadow Bolt";

        readonly string pullingSpell;
        Location currentWaypoint;

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull)
        {
            if (Spellbook.Instance.IsSpellReady(CurseOfAgony))
                pullingSpell = CurseOfAgony;
            else
                pullingSpell = ShadowBolt;
        }

        public void Update()
        {

            if (ObjectManager.Instance.Pet == null && (Spellbook.Instance.IsSpellReady(SummonImp) || Spellbook.Instance.IsSpellReady(SummonVoidwalker)))
            {
                Container.Player.StopAllMovement();
                BotTasks.Push(new SummonVoidwalkerTask(Container, BotTasks));
                return;
            }

            float distanceToTarget = Container.Player.Location.GetDistanceTo(Container.HostileTarget.Location);
            if (distanceToTarget < 27 && Container.Player.IsCasting && Spellbook.Instance.IsSpellReady(pullingSpell))
            {
                if (Container.Player.MovementState != MovementFlags.None)
                    Container.Player.StopAllMovement();

                if (Wait.For("AfflictionWarlockPullDelay", 250))
                {
                    Container.Player.StopAllMovement();
                    Lua.Instance.Execute($"CastSpellByName('{pullingSpell}')");
                    BotTasks.Pop();
                    BotTasks.Push(new PvERotationTask(Container, BotTasks));
                }

                return;
            }

            Location[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);
            if (nextWaypoint.Length > 1)
            {
                currentWaypoint = nextWaypoint[1];
            }
            else
            {
                BotTasks.Pop();
                return;
            }

            Container.Player.MoveToward(currentWaypoint);
        }
    }
}
