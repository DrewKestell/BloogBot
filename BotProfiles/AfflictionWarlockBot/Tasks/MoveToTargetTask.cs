using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace AfflictionWarlockBot
{
    class MoveToTargetTask : IBotTask
    {
        const string SummonImp = "Summon Imp";
        const string SummonVoidwalker = "Summon Voidwalker";
        const string CurseOfAgony = "Curse of Agony";
        const string ShadowBolt = "Shadow Bolt";

        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        readonly string pullingSpell;

        Location currentWaypoint;
        WoWUnit target;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.target = target;
            player = ObjectManager.Instance.Player;
            currentWaypoint = player.Location;
            stuckHelper = new StuckHelper(container, botTasks);

            if (Spellbook.Instance.IsSpellReady(CurseOfAgony))
                pullingSpell = CurseOfAgony;
            else
                pullingSpell = ShadowBolt;
        }

        public void Update()
        {
            if (ObjectManager.Instance.Hostiles.Count > 0)
            {
                WoWUnit potentialNewTarget = ObjectManager.Instance.Hostiles.First();

                if (potentialNewTarget != null && potentialNewTarget.Guid != target.Guid)
                {
                    target = potentialNewTarget;
                    player.SetTarget(potentialNewTarget);
                }
            }

            if (ObjectManager.Instance.Pet == null && (Spellbook.Instance.IsSpellReady(SummonImp) || Spellbook.Instance.IsSpellReady(SummonVoidwalker)))
            {
                player.StopAllMovement();
                botTasks.Push(new SummonVoidwalkerTask(container, botTasks));
                return;
            }

            stuckHelper.CheckIfStuck();

            var distanceToTarget = player.Location.GetDistanceTo(target.Location);
            if (distanceToTarget < 27 && player.IsCasting && Spellbook.Instance.IsSpellReady(pullingSpell))
            {
                if (player.MovementState != MovementFlags.None)
                    player.StopAllMovement();

                if (Wait.For("AfflictionWarlockPullDelay", 250))
                {
                    player.StopAllMovement();
                    Lua.Instance.Execute($"CastSpellByName('{pullingSpell}')");
                    botTasks.Pop();
                    botTasks.Push(new PvERotationTask(container, botTasks));
                }

                return;
            }

            var nextWaypoint = SocketClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, player.Location, target.Location, false);
            if (nextWaypoint.Length > 1)
            {
                currentWaypoint = nextWaypoint[1];
            }
            else
            {
                botTasks.Pop();
                return;
            }

            player.MoveToward(currentWaypoint);
        }
    }
}
