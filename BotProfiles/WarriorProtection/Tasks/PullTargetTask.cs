using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;

namespace WarriorProtection.Tasks
{
    internal class PullTargetTask : BotTask, IBotTask
    {
        internal PullTargetTask(IBotContext botContext) : base(botContext)
        {
            IWoWItem rangedWeapon = ObjectManager.GetEquippedItem(EquipSlot.Ranged);
            if (rangedWeapon == null)
            {
                List<IWoWItem> knives = ObjectManager.Items.Where(x => x.ItemId == 2947).ToList();
                uint knivesCount = (uint)knives.Sum(x => x.StackCount);

                if (knivesCount < 200)
                {
                    ObjectManager.SendChatMessage(".additem 2947 " + (200 - knivesCount));
                }
            }
            else if (rangedWeapon.Info.ItemSubclass == ItemSubclass.Bow)
            {
                List<IWoWItem> arrows = ObjectManager.Items.Where(x => x.ItemId == 2512).ToList();
                uint arrowsCount = (uint)arrows.Sum(x => x.StackCount);

                if (arrowsCount < 200)
                {
                    ObjectManager.SendChatMessage(".additem 2512 " + (200 - arrowsCount));
                }
            }
            else if (rangedWeapon.Info.ItemSubclass == ItemSubclass.Gun)
            {
                List<IWoWItem> shots = ObjectManager.Items.Where(x => x.ItemId == 2516).ToList();
                uint shotsCount = (uint)shots.Sum(x => x.StackCount);

                if (shotsCount < 200)
                {
                    ObjectManager.SendChatMessage(".additem 2516 " + (200 - shotsCount));
                }
            }
            
            IWoWUnit nearestHostile = ObjectManager.Hostiles.Where(x => !x.IsInCombat).OrderBy(x => x.Position.DistanceTo(ObjectManager.Player.Position)).First();
            float distance = nearestHostile.Position.DistanceTo(ObjectManager.Player.Position) < 15 ? 30 : 15;

            Position tankSpot;

            //if (Container.State.VisitedWaypoints.Count(x => Container.PathfindingClient.GetPathingDistance(ObjectManager.MapId, ObjectManager.Player.Position, x, true) > distance) > 0)
            //    tankSpot = Container.State.VisitedWaypoints.Where(x => Container.PathfindingClient.GetPathingDistance(ObjectManager.MapId, ObjectManager.Player.Position, x, true) > 15)
            //        .OrderBy(x => Container.PathfindingClient.GetPathingDistance(ObjectManager.MapId, ObjectManager.Player.Position, x, true))
            //        .First();
            //else
            //    tankSpot = ObjectManager.Player.Position;
        }

        public void Update()
        {
            if (ObjectManager.GetTarget(ObjectManager.Player) == null || ObjectManager.GetTarget(ObjectManager.Player).HealthPercent == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Aggressors.Any())
            {
                BotTasks.Pop();
                BotTasks.Push(Container.CreatePvERotationTask(BotContext));
                return;
            }

            if (ObjectManager.GetTarget(ObjectManager.Player).Health == 0 || (!ObjectManager.Player.InLosWith(ObjectManager.GetTarget(ObjectManager.Player)) && Wait.For("LosTimer", 2000)))
            {
                if (ObjectManager.Player.IsMoving)
                    ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(ObjectManager.GetTarget(ObjectManager.Player).Position);

            if (distanceToTarget < 25 && distanceToTarget > 8 && ObjectManager.Player.InLosWith(ObjectManager.GetTarget(ObjectManager.Player)))
            {
                ObjectManager.Player.StopAllMovement();

                if (!ObjectManager.Player.IsCasting)
                    ObjectManager.Player.CastSpell("Shoot Bow");

            }
            else
            {
                Position[] locations = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.GetTarget(ObjectManager.Player).Position, true);

                if (locations.Where(loc => loc.DistanceTo(ObjectManager.Player.Position) > 3).Any())
                {
                    Position position = locations.Where(loc => loc.DistanceTo(ObjectManager.Player.Position) > 3).ToArray()[0];
                    ObjectManager.Player.MoveToward(position);
                }
                else
                {
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }
            }
        }
    }
}
