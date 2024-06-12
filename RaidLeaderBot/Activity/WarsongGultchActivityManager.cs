using System.Linq;
using RaidMemberBot.Models.Dto;

namespace RaidLeaderBot.Activity
{
    public class WarsongGultchActivityManager : ActivityManager
    {
        private string BattlemasterNPCName => PartyMembersToStates.Keys.First().IsAlliance ? "Elfarran" : "Brakgul";
        protected override bool ActivityRunCondition => PartyMembersToStates.All(x => x.Value.InParty && x.Value.IsReadyToStart);

        public WarsongGultchActivityManager(ActivityType activityType, int portNumber, int mapId) : base(activityType, portNumber, mapId)
        {

        }

        protected override void CheckForCommand(RaidMemberViewModel raidMemberViewModel, CharacterState newCharacterState)
        {
            if (PartyMembersToStates.All(x => x.Value.InParty || x.Value.InRaid))
            {
                if (PartyMembersToStates.All(x => x.Value.IsReadyToStart))
                {
                    if (PartyMembersToStates.Any(x => x.Value.MapId != MapId))
                    {
                        if (newCharacterState.WoWUnits.Values.Count(x => x.StartsWith(BattlemasterNPCName)) < 1)
                            NextCommand[newCharacterState.ProcessId] = new InstanceCommand()
                            {
                                CommandAction = CommandAction.ExecuteLuaCommand,
                                CommandParam1 = $"SendChatMessage(\'.go creature {BattlemasterNPCName}\')",
                            };
                        else if (newCharacterState.Guid == RaidLeader.Guid && !newCharacterState.InBGQueue && PartyMembersToStates.Values.All(x => x.WoWUnits.Values.Any(x => x.StartsWith(BattlemasterNPCName))))
                        {
                            NextCommand[newCharacterState.ProcessId] = new InstanceCommand()
                            {
                                CommandAction = CommandAction.QueuePvP,
                                CommandParam1 = "WSG",
                                CommandParam2 = BattlemasterNPCName
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
                    else
                    {
                        NextCommand[newCharacterState.ProcessId] = new InstanceCommand()
                        {
                            CommandAction = CommandAction.None,
                        };
                    }
                }
                else
                {
                    if (newCharacterState.HasStarted)
                    {
                        NextCommand[newCharacterState.ProcessId] = new InstanceCommand()
                        {
                            CommandAction = CommandAction.None,
                        };
                    }
                    else
                    {
                        NextCommand[newCharacterState.ProcessId] = new InstanceCommand()
                        {
                            CommandAction = CommandAction.BeginBattleGrounds,
                            CommandParam1 = "WSG"
                        };
                    }
                }
            }
        }
    }
}
