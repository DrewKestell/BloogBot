using System;
using System.Linq;
using RaidMemberBot.Models;
using RaidMemberBot.Models.Dto;

namespace RaidLeaderBot.Activity
{
    public class DungeonActivityManager : ActivityManager
    {
        protected override bool ActivityRunCondition => PartyMembersToStates.All(x => x.Value.InParty && x.Value.IsReadyToStart);

        public DungeonActivityManager(int portNumber, int mapId) : base(portNumber, mapId)
        {

        }

        protected override void CheckForCommand(RaidMemberViewModel raidMemberViewModel, CharacterState newCharacterState)
        {
            Console.WriteLine("CheckForCommand");
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
                                InstanceCommand goToCommand = new InstanceCommand()
                                {
                                    CommandAction = CommandAction.TeleTo,
                                    CommandParam1 = areaTriggerTeleport.TargetPositionX.ToString(),
                                    CommandParam2 = areaTriggerTeleport.TargetPositionY.ToString(),
                                    CommandParam3 = areaTriggerTeleport.TargetPositionZ.ToString(),
                                    CommandParam4 = MapId.ToString(),
                                };
                                NextCommand[newCharacterState.ProcessId] = goToCommand;
                            }
                        }
                        else
                        {
                            if (PartyMembersToStates.All(x => x.Value.IsReadyToStart) && !newCharacterState.HasStarted)
                            {
                                InstanceCommand beginDungeon = new InstanceCommand()
                                {
                                    CommandAction = CommandAction.BeginDungeon,
                                };

                                NextCommand[newCharacterState.ProcessId] = beginDungeon;
                            }
                            else if (RaidLeader.Guid != newCharacterState.Guid
                                && Math.Round((decimal)newCharacterState.TankPosition.X, 2) != Math.Round((decimal)RaidLeader.TankPosition.X, 2)
                                && Math.Round((decimal)newCharacterState.TankPosition.Y, 2) != Math.Round((decimal)RaidLeader.TankPosition.Y, 2)
                                && Math.Round((decimal)newCharacterState.TankPosition.Z, 2) != Math.Round((decimal)RaidLeader.TankPosition.Z, 2)
                                && Math.Round((decimal)newCharacterState.TankFacing, 2) != Math.Round((decimal)RaidLeader.TankFacing, 2))
                            {
                                InstanceCommand setCombatLocation = new InstanceCommand()
                                {
                                    CommandAction = CommandAction.SetCombatLocation,
                                    CommandParam1 = RaidLeader.TankPosition.X.ToString(),
                                    CommandParam2 = RaidLeader.TankPosition.Y.ToString(),
                                    CommandParam3 = RaidLeader.TankPosition.Z.ToString(),
                                    CommandParam4 = RaidLeader.TankFacing.ToString()
                                };

                                NextCommand[newCharacterState.ProcessId] = setCombatLocation;
                            }
                            else if (RaidLeader.Guid != newCharacterState.Guid
                                && RaidLeader.TankInPosition != newCharacterState.TankInPosition)
                            {

                                InstanceCommand setTankPosition = new InstanceCommand()
                                {
                                    CommandAction = CommandAction.SetTankInPosition,
                                    CommandParam1 = RaidLeader.TankInPosition.ToString(),
                                };

                                NextCommand[newCharacterState.ProcessId] = setTankPosition;
                            }
                            else if (!newCharacterState.InCombat && FindMissingTalents(raidMemberViewModel, newCharacterState, out int talentSpellId))
                            {
                                InstanceCommand addTalent = new InstanceCommand()
                                {
                                    CommandAction = CommandAction.AddTalent,
                                    CommandParam1 = talentSpellId.ToString(),
                                };

                                NextCommand[newCharacterState.ProcessId] = addTalent;
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
                InstanceCommand stopCommand = new InstanceCommand()
                {
                    CommandAction = CommandAction.FullStop
                };

                NextCommand[newCharacterState.ProcessId] = stopCommand;
            }
        }
    }
}
