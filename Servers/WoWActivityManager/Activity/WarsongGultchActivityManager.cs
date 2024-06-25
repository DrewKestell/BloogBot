using WoWActivityManager.Models;
using WoWClientBot.Models;

namespace WoWActivityManager.Activity
{
    public class WarsongGultchActivityManager(ActivityPreset activityPreset) : ActivityManager(activityPreset)
    {
        private string BattlemasterNPCName => PartyMembersToStates.Values.First().IsAlliance ? "Elfarran" : "Brakgul";
        protected override bool ActivityRunCondition => PartyMembersToStates.All(x => x.Value.InParty && x.Value.IsReadyToStart);

        protected override void CheckForCommand(ActivityMemberPreset activityMemberViewModel, CharacterState newCharacterState)
        {
            if (PartyMembersToStates.All(x => x.Value.InParty || x.Value.InRaid))
            {
                if (PartyMembersToStates.All(x => x.Value.IsReadyToStart))
                {
                    if (PartyMembersToStates.Any(x => x.Value.MapId != Preset.MapId))
                    {
                        if (newCharacterState.WoWUnits.Values.Count(x => x.StartsWith(BattlemasterNPCName)) < 1)
                            NextCommand[newCharacterState.ProcessId] = new CharacterCommand()
                            {
                                CharacterAction = CharacterAction.ExecuteLuaCommand,
                                CommandParam1 = $"SendChatMessage(\'.go creature {BattlemasterNPCName}\')",
                            };
                        else if (newCharacterState.Guid == RaidLeader.Guid && !newCharacterState.InBGQueue && PartyMembersToStates.Values.All(x => x.WoWUnits.Values.Any(x => x.StartsWith(BattlemasterNPCName))))
                        {
                            NextCommand[newCharacterState.ProcessId] = new CharacterCommand()
                            {
                                CharacterAction = CharacterAction.QueuePvP,
                                CommandParam1 = "WSG",
                                CommandParam2 = BattlemasterNPCName
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
                    else
                    {
                        NextCommand[newCharacterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.None,
                        };
                    }
                }
                else
                {
                    if (newCharacterState.HasStarted)
                    {
                        NextCommand[newCharacterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.None,
                        };
                    }
                    else
                    {
                        NextCommand[newCharacterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.BeginBattleGrounds,
                            CommandParam1 = "WSG"
                        };
                    }
                }
            }
        }
    }
}
