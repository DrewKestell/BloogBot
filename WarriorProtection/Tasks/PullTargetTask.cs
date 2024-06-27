using WoWActivityMember.Tasks;
using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;
using static WoWActivityMember.Constants.Enums;

namespace ProtectionWarriorBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull)
        {
            WoWItem rangedWeapon = Inventory.GetEquippedItem(EquipSlot.Ranged);
            if (rangedWeapon == null)
            {
                List<WoWItem> knives = ObjectManager.Items.Where(x => x.ItemId == 2947).ToList();
                int knivesCount = knives.Sum(x => x.StackCount);

                if (knivesCount < 200)
                {
                    Functions.LuaCall($"SendChatMessage('.additem 2947 {200 - knivesCount}')");
                }
            }
            else if (rangedWeapon.Info.ItemSubclass == ItemSubclass.Bow)
            {
                List<WoWItem> arrows = ObjectManager.Items.Where(x => x.ItemId == 2512).ToList();
                int arrowsCount = arrows.Sum(x => x.StackCount);

                if (arrowsCount < 200)
                {
                    Functions.LuaCall($"SendChatMessage('.additem 2512 {200 - arrowsCount}')");
                }
            }
            else if (rangedWeapon.Info.ItemSubclass == ItemSubclass.Gun)
            {
                List<WoWItem> shots = ObjectManager.Items.Where(x => x.ItemId == 2516).ToList();
                int shotsCount = shots.Sum(x => x.StackCount);

                if (shotsCount < 200)
                {
                    Functions.LuaCall($"SendChatMessage('.additem 2516 {200 - shotsCount}')");
                }
            }
            
            WoWUnit nearestHostile = ObjectManager.Hostiles.Where(x => !x.IsInCombat).OrderBy(x => x.Position.DistanceTo(ObjectManager.Player.Position)).First();
            float distance = nearestHostile.Position.DistanceTo(ObjectManager.Player.Position) < 15 ? 30 : 15;

            Position tankSpot;

            //if (Container.State.VisitedWaypoints.Count(x => Navigation.Instance.CalculatePathingDistance(ObjectManager.MapId, ObjectManager.Player.Position, x, true) > distance) > 0)
            //    tankSpot = Container.State.VisitedWaypoints.Where(x => Navigation.Instance.CalculatePathingDistance(ObjectManager.MapId, ObjectManager.Player.Position, x, true) > 15)
            //        .OrderBy(x => Navigation.Instance.CalculatePathingDistance(ObjectManager.MapId, ObjectManager.Player.Position, x, true))
            //        .First();
            //else
            //    tankSpot = ObjectManager.Player.Position;
        }

        public void Update()
        {
            if (ObjectManager.Player.Target == null || ObjectManager.Player.Target.HealthPercent == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Aggressors.Count > 0)
            {
                BotTasks.Pop();
                BotTasks.Push(Container.CreatePvERotationTask(Container, BotTasks));
                return;
            }

            if (ObjectManager.Player.Target.Health == 0 || (!ObjectManager.Player.InLosWith(ObjectManager.Player.Target) && Wait.For("LosTimer", 2000)))
            {
                if (ObjectManager.Player.IsMoving)
                    ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position);

            if (distanceToTarget < 25 && distanceToTarget > 8 && ObjectManager.Player.InLosWith(ObjectManager.Player.Target))
            {
                ObjectManager.Player.StopAllMovement();

                if (!ObjectManager.Player.IsCasting)
                    Functions.LuaCall("CastSpellByName('Shoot Bow')");

            }
            else
            {
                Position[] locations = Navigation.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);

                if (locations.Where(loc => loc.DistanceTo(ObjectManager.Player.Position) > 3).Count() > 0)
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
