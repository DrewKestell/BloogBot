using System;
using System.Linq;
using RaidMemberBot.Models;
using RaidMemberBot.Models.Dto;

namespace RaidLeaderBot.Activity
{
    public class DungeonActivityManager : ActivityManager
    {
        protected override bool ActivityRunCondition => PartyMembersToStates.All(x => x.Value.InParty && x.Value.IsReadyToStart);

        public DungeonActivityManager(ActivityType activityType, int portNumber, int mapId) : base(activityType, portNumber, mapId)
        {

        }

        protected override void CheckForCommand(RaidMemberViewModel raidMemberViewModel, CharacterState newCharacterState)
        {
            if (raidMemberViewModel.ShouldRun)
            {
                if (BotPreparedToStart(raidMemberViewModel, newCharacterState))
                {
                    if (PartyMembersToStates.All(x => x.Value.InParty && x.Value.IsReadyToStart))
                    {
                        if (PartyMembersToStates.Any(x => x.Value.MapId != MapId))
                        {
                            if (newCharacterState.MapId != MapId)
                            {
                                AreaTriggerTeleport areaTriggerTeleport = MangosRepository.GetAreaTriggerTeleportByMapId(MapId);
                                NextCommand[newCharacterState.ProcessId] = new InstanceCommand()
                                {
                                    CommandAction = CommandAction.TeleTo,
                                    CommandParam1 = areaTriggerTeleport.TargetPositionX.ToString(),
                                    CommandParam2 = areaTriggerTeleport.TargetPositionY.ToString(),
                                    CommandParam3 = areaTriggerTeleport.TargetPositionZ.ToString(),
                                    CommandParam4 = MapId.ToString(),
                                };
                            }
                        }
                        else
                        {
                            if (PartyMembersToStates.All(x => x.Value.IsReadyToStart) && !newCharacterState.HasStarted)
                            {
                                NextCommand[newCharacterState.ProcessId] = new InstanceCommand()
                                {
                                    CommandAction = CommandAction.BeginDungeon,
                                };
                            }
                            else if (RaidLeader.Guid != newCharacterState.Guid
                                && Math.Round((decimal)newCharacterState.TankPosition.X, 2) != Math.Round((decimal)RaidLeader.TankPosition.X, 2)
                                && Math.Round((decimal)newCharacterState.TankPosition.Y, 2) != Math.Round((decimal)RaidLeader.TankPosition.Y, 2)
                                && Math.Round((decimal)newCharacterState.TankPosition.Z, 2) != Math.Round((decimal)RaidLeader.TankPosition.Z, 2)
                                && Math.Round((decimal)newCharacterState.TankFacing, 2) != Math.Round((decimal)RaidLeader.TankFacing, 2))
                            {
                                NextCommand[newCharacterState.ProcessId] = new InstanceCommand()
                                {
                                    CommandAction = CommandAction.SetCombatLocation,
                                    CommandParam1 = RaidLeader.TankPosition.X.ToString(),
                                    CommandParam2 = RaidLeader.TankPosition.Y.ToString(),
                                    CommandParam3 = RaidLeader.TankPosition.Z.ToString(),
                                    CommandParam4 = RaidLeader.TankFacing.ToString()
                                };
                            }
                            else if (RaidLeader.Guid != newCharacterState.Guid
                                && RaidLeader.TankInPosition != newCharacterState.TankInPosition)
                            {
                                NextCommand[newCharacterState.ProcessId] = new InstanceCommand()
                                {
                                    CommandAction = CommandAction.SetTankInPosition,
                                    CommandParam1 = RaidLeader.TankInPosition.ToString(),
                                };
                            }
                            else if (!newCharacterState.InCombat && FindMissingTalents(raidMemberViewModel, newCharacterState, out int talentSpellId))
                            {
                                NextCommand[newCharacterState.ProcessId] = new InstanceCommand()
                                {
                                    CommandAction = CommandAction.AddTalent,
                                    CommandParam1 = talentSpellId.ToString(),
                                };
                            }
                            else
                            {
                                NextCommand[newCharacterState.ProcessId] = new InstanceCommand()
                                {
                                    CommandAction = CommandAction.None,
                                };
                            }
                        }
                    }
                }                
            }
            else
            {
                NextCommand[newCharacterState.ProcessId] = new InstanceCommand()
                {
                    CommandAction = CommandAction.FullStop
                };
            }
        }
    }
}
