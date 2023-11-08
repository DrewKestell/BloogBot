using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
{
    public class UseItemOnUnitTask : BotTask, IBotTask
    {
        readonly WoWItem usableItem;

        bool itemUsed;

        public UseItemOnUnitTask(IClassContainer container, Stack<IBotTask> botTasks, WoWItem wowItem) : base(container, botTasks, TaskType.Ordinary)
        {
            usableItem = wowItem;
            itemUsed = false;
        }

        public void Update()
        {
            if (Container.Player.Location.GetDistanceTo(Container.HostileTarget.Location) < 3)
            {
                if (!itemUsed)
                {
                    Container.Player.SetTarget(Container.HostileTarget.Guid);
                    Container.Player.StopMovement(Constants.Enums.ControlBits.Nothing);
                    usableItem.Use();

                    itemUsed = true;
                }
                else if (Wait.For("ItemUseAnim", 1000))
                {
                    BotTasks.Pop();
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
