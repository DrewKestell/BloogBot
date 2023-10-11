using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace ArcaneMageBot
{
    class MoveToTargetTask : IBotTask
    {
        const string Fireball = "Fireball";
        const string Frostbolt = "Frostbolt";

        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        readonly string pullingSpell;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.target = target;
            player = ObjectManager.Instance.Player;
            stuckHelper = new StuckHelper(container, botTasks);

            if (Spellbook.Instance.IsSpellReady(Frostbolt))
                pullingSpell = Frostbolt;
            else
                pullingSpell = Fireball;
        }

        public void Update()
        {
            if (target.TappedByOther)
            {
                player.StopAllMovement();
                botTasks.Pop();
                return;
            }

            stuckHelper.CheckIfStuck();

            var distanceToTarget = player.Location.GetDistanceTo(target.Location);
            if (distanceToTarget < 27)
            {
                if (player.IsMoving)
                    player.StopAllMovement();

                if (player.IsCasting && Spellbook.Instance.IsSpellReady(pullingSpell) && Wait.For("ArcaneMagePull", 500))
                {
                    player.StopAllMovement();
                    Wait.RemoveAll();
                    Lua.Instance.Execute($"CastSpellByName('{pullingSpell}')");
                    botTasks.Pop();
                    botTasks.Push(new PvERotationTask(container, botTasks));
                    return;
                }
            }
            else
            {
                var nextWaypoint = SocketClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, player.Location, target.Location, false);
                player.MoveToward(nextWaypoint[0]);
            }
        }
    }
}
