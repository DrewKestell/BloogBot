// Friday owns this file!

using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace BeastMasterHunterBot
{
    class MoveToTargetTask : IBotTask
    {
        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;

        const string GunLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Auto Shot') end";
        const string SerpentSting = "Serpent Sting";
        const string AspectOfTheMonkey = "Aspect Of The Monkey";
        const string AspectOfTheCheetah = "Aspect Of The Cheetah";

        WoWUnit target;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.botTasks = botTasks;
            this.container = container;
            this.target = target;
            player = ObjectManager.Instance.Player;
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

            if (player.Location.GetDistanceTo(target.Location) < 33)
            {
                player.StopAllMovement();
                Lua.Instance.Execute(GunLuaScript);
                botTasks.Pop();
                botTasks.Push(new PvERotationTask(container, botTasks));
                return;
            } else
            {
                var nextWaypoint = SocketClient.Instance.CalculatePath(player.MapId, player.Location, target.Location, false);
                player.MoveToward(nextWaypoint[1]);
            }
        }
    }
}
