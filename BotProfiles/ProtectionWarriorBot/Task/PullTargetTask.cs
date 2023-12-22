using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

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

            if (ObjectManager.Player.Target.Health == 0 || (!ObjectManager.Player.InLosWith(ObjectManager.Player.Target.Position) && Wait.For("LosTimer", 2000)))
            {
                if (ObjectManager.Player.IsMoving)
                    ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position);

            if (distanceToTarget < 25 && distanceToTarget > 8 && ObjectManager.Player.InLosWith(ObjectManager.Player.Target.Position))
            {
                ObjectManager.Player.StopAllMovement();

                if (!ObjectManager.Player.IsCasting)
                    Functions.LuaCall("CastSpellByName('Shoot Bow')");

            }
            else
            {
                Position[] locations = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);

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
