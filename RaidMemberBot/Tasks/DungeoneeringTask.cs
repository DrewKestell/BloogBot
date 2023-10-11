using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Models;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.AI.SharedStates
{
    public class DungeoneeringTask : BotTask, IBotTask
    {
        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        List<Creature> encounters;

        public DungeoneeringTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;

            encounters = SqliteRepository.GetCreaturesByMapId((int)ObjectManager.Instance.Player.MapId)
                .OrderBy(x => x.SpawnLocation.DistanceToPlayer())
                .ToList();

            WoWEventHandler.Instance.OnUnitKilled += Instance_OnUnitKilled;
        }

        private void Instance_OnUnitKilled(object sender, EventArgs e)
        {
            Console.WriteLine($"Unit Killed");
        }

        public void Update()
        {
            if (ObjectManager.Instance.Aggressors.Count > 0)
            {
                botTasks.Push(container.CreatePvERotationTask(container, botTasks));
                return;
            }

            if (ObjectManager.Instance.PartyLeader?.Guid == ObjectManager.Instance.Player.Guid && ObjectManager.Instance.PartyMembers.All(x => x.ManaPercent < 0 || x.ManaPercent > 80))
            {
                if (ObjectManager.Instance.Hostiles.Where(x => x.Location.DistanceToPlayer() < 25 && ObjectManager.Instance.Player.InLosWith(x)).Count() > 0)
                {
                    WoWUnit wowUnit = ObjectManager.Instance.Hostiles.Where(x => x.Location.DistanceToPlayer() < 25 && ObjectManager.Instance.Player.InLosWith(x)).OrderBy(x => x.Location.DistanceToPlayer()).First();
                    botTasks.Push(container.CreatePullTargetTask(container, botTasks, wowUnit));
                }
                else if (encounters.Count > 0)
                {
                    Location[] locations = SocketClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, ObjectManager.Instance.Player.Location, encounters[0].SpawnLocation, false);

                    if (locations.Length > 1)
                    {
                        ObjectManager.Instance.Player.MoveToward(locations[1]);
                    }
                }
            }
            else
            {
                if (ObjectManager.Instance.PartyLeader == null)
                {
                    Location[] locations = SocketClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, ObjectManager.Instance.Player.Location, new Location(), false);

                    if (locations.Length > 1)
                    {
                        ObjectManager.Instance.Player.MoveToward(locations[1]);
                    }
                }
                else if (ObjectManager.Instance.PartyLeader?.Location.DistanceToPlayer() > 15)
                {
                    Location[] locations = SocketClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, ObjectManager.Instance.Player.Location, ObjectManager.Instance.PartyLeader.Location, false);

                    if (locations.Length > 1)
                    {
                        ObjectManager.Instance.Player.MoveToward(locations[1]);
                    }
                }
                else
                {
                    ObjectManager.Instance.Player.StopAllMovement();
                }

                botTasks.Push(container.CreateBuffTask(container, botTasks));
                botTasks.Push(container.CreateRestTask(container, botTasks));
            }

            encounters.RemoveAll(x => x.SpawnLocation.DistanceToPlayer() < 15);
            encounters = encounters.OrderBy(x => x.SpawnLocation.DistanceToPlayer()).ToList();
        }
    }
}
