using MaNGOSDBDomain.Models;
using WoWActivityManager.Models;
using WoWClientBot.Models;

namespace WoWActivityManager.Activity
{
    public class DungeonActivityManager(ActivityPreset activityPreset) : ActivityManager(activityPreset)
    {
        protected override bool ActivityRunCondition => PartyMembersToStates.All(x => x.Value.InParty && x.Value.IsReadyToStart);

        protected override void CheckForCommand(ActivityMemberPreset activityMemberViewModel, CharacterState newCharacterState)
        {
            if (activityMemberViewModel.ShouldRun)
            {
                if (BotPreparedToStart(activityMemberViewModel, newCharacterState))
                {
                    if (PartyMembersToStates.All(x => x.Value.InParty && x.Value.IsReadyToStart))
                    {
                        if (PartyMembersToStates.Any(x => x.Value.MapId != Preset.MapId))
                        {
                            if (newCharacterState.MapId != Preset.MapId)
                            {
                                AreaTriggerTeleport areaTriggerTeleport = MangosRepository.GetAreaTriggerTeleportByMapId(Preset.MapId);
                                NextCommand[newCharacterState.ProcessId] = new CharacterCommand()
                                {
                                    CharacterAction = CharacterAction.TeleTo,
                                    CommandParam1 = areaTriggerTeleport.TargetPositionX.ToString(),
                                    CommandParam2 = areaTriggerTeleport.TargetPositionY.ToString(),
                                    CommandParam3 = areaTriggerTeleport.TargetPositionZ.ToString(),
                                    CommandParam4 = Preset.MapId.ToString(),
                                };
                            }
                        }
                        else
                        {
                            if (PartyMembersToStates.All(x => x.Value.IsReadyToStart) && !newCharacterState.HasStarted)
                            {
                                NextCommand[newCharacterState.ProcessId] = new CharacterCommand()
                                {
                                    CharacterAction = CharacterAction.BeginDungeon,
                                };
                            }
                            else if (RaidLeader.Guid != newCharacterState.Guid
                                && Math.Round((decimal)newCharacterState.TankPosition.X, 2) != Math.Round((decimal)RaidLeader.TankPosition.X, 2)
                                && Math.Round((decimal)newCharacterState.TankPosition.Y, 2) != Math.Round((decimal)RaidLeader.TankPosition.Y, 2)
                                && Math.Round((decimal)newCharacterState.TankPosition.Z, 2) != Math.Round((decimal)RaidLeader.TankPosition.Z, 2)
                                && Math.Round((decimal)newCharacterState.TankFacing, 2) != Math.Round((decimal)RaidLeader.TankFacing, 2))
                            {
                                NextCommand[newCharacterState.ProcessId] = new CharacterCommand()
                                {
                                    CharacterAction = CharacterAction.SetCombatLocation,
                                    CommandParam1 = RaidLeader.TankPosition.X.ToString(),
                                    CommandParam2 = RaidLeader.TankPosition.Y.ToString(),
                                    CommandParam3 = RaidLeader.TankPosition.Z.ToString(),
                                    CommandParam4 = RaidLeader.TankFacing.ToString()
                                };
                            }
                            else if (RaidLeader.Guid != newCharacterState.Guid
                                && RaidLeader.TankInPosition != newCharacterState.TankInPosition)
                            {
                                NextCommand[newCharacterState.ProcessId] = new CharacterCommand()
                                {
                                    CharacterAction = CharacterAction.SetTankInPosition,
                                    CommandParam1 = RaidLeader.TankInPosition.ToString(),
                                };
                            }
                            else if (!newCharacterState.InCombat && FindMissingTalents(activityMemberViewModel, newCharacterState, out int talentSpellId))
                            {
                                NextCommand[newCharacterState.ProcessId] = new CharacterCommand()
                                {
                                    CharacterAction = CharacterAction.AddTalent,
                                    CommandParam1 = talentSpellId.ToString(),
                                };
                            }
                            else
                            {
                                NextCommand[newCharacterState.ProcessId] = new CharacterCommand()
                                {
                                    CharacterAction = CharacterAction.None,
                                };
                            }
                        }
                    }
                }                
            }
            else
            {
                NextCommand[newCharacterState.ProcessId] = new CharacterCommand()
                {
                    CharacterAction = CharacterAction.FullStop
                };
            }
        }
    }
}
