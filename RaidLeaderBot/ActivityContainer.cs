using RaidMemberBot.Models.Dto;
using System.Diagnostics;
using System.Collections.Generic;
using static RaidLeaderBot.WinImports;

namespace RaidLeaderBot
{
    public class ActivityContainer
    {
        private readonly CommandSockerServer _socketServer;
        public ActivityType Activity { get; }
        public CharacterState RaidLeader { set; get; }
        public int RaidSize { get; }
        public int MapId { get; }
        public int HostMapId { get; }
        private List<RaidMemberPreset> _partyMemberPresets = new List<RaidMemberPreset>();
        public List<RaidMemberPreset> RaidMemberPresets
        {
            get
            {
                return _partyMemberPresets;
            }
            set
            {
                _partyMemberPresets = value;
            }
        }
        public readonly CharacterState[] _characterStates = {
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
        };
        public ActivityContainer()
        {

        }

        private void OnInstanceUpdate(CharacterState update)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    ConsumeMessage(update);
            //});
        }
        public void ConsumeMessage(CharacterState characterState)
        {
            CheckForCommand(UpdateCharacterState(characterState), characterState);
        }

        private int UpdateCharacterState(CharacterState characterState)
        {
            for (int i = 0; i < RaidMemberPresets.Count; i++)
            {
                if (_characterStates[i].ProcessId == characterState.ProcessId)
                {
                    _characterStates[i] = characterState;

                    if (!_characterStates[i].IsConnected)
                    {
                        _characterStates[i].ProcessId = 0;
                    }
                    else
                    {
                        SetWindowText(Process.GetProcessById((int)_characterStates[i].ProcessId).MainWindowHandle, $"WoW - {_characterStates[i].CharacterName}");
                    }

                    return i;
                }
            }
            for (int i = 0; i < RaidMemberPresets.Count; i++)
            {
                if (_characterStates[i].ProcessId == 0
                    && RaidMemberPresets[i].Class == _characterStates[i].Class
                    && RaidMemberPresets[i].Race == _characterStates[i].Race)
                {
                    _characterStates[i] = characterState;
                    return i;
                }
            }
            for (int i = 0; i < RaidMemberPresets.Count; i++)
            {
                if (_characterStates[i].ProcessId == 0)
                {
                    _characterStates[i] = characterState;
                    return i;
                }
            }
            return -1;
        }
        private void CheckForCommand(int index, CharacterState characterState)
        {
            //if (Players[index].ShouldRun)
            //{
            //    if (characterState.ProcessId > 0)
            //    {
            //        if (characterState.Guid == 0
            //            || characterState.Class != RaidMemberPresets[index].Class
            //            || characterState.Race != RaidMemberPresets[index].Race
            //            || characterState.CharacterIndex != RaidMemberPresets[index].CharacterIndex)
            //        {
            //            InstanceCommand loginCommand = new InstanceCommand()
            //            {
            //                CommandAction = CommandAction.SetCharacterParams,
            //                CommandParam1 = RaidMemberPresets[index].Class.ToString(),
            //                CommandParam2 = RaidMemberPresets[index].Race,
            //                CommandParam3 = RaidMemberPresets[index].CharacterIndex.ToString()
            //            };

            //            _socketServer.SendCommandToProcess(characterState.ProcessId, loginCommand);
            //            return;

            //        }
            //        else if (string.IsNullOrEmpty(characterState.CurrentActivity))
            //        {
            //            InstanceCommand setActivityCommand = new InstanceCommand()
            //            {
            //                CommandAction = CommandAction.SetActivity,
            //                CommandParam1 = Activity.ToString(),
            //                CommandParam2 = _ACTIVITY_TELE_IDS[Activity].Item1.ToString()
            //            };

            //            _socketServer.SendCommandToProcess(characterState.ProcessId, setActivityCommand);
            //            return;
            //        }
            //        else if (RaidLeader == null || RaidLeader.ProcessId == 0)
            //        {
            //            RaidLeader = characterState;

            //            InstanceCommand setLeaderCommand = new InstanceCommand()
            //            {
            //                CommandAction = CommandAction.SetRaidLeader,
            //                CommandParam1 = RaidLeader.CharacterName,
            //            };

            //            _socketServer.SendCommandToProcess(characterState.ProcessId, setLeaderCommand);
            //            return;
            //        }
            //        else if (RaidLeader.CharacterName != characterState.RaidLeader)
            //        {
            //            InstanceCommand setLeaderCommand = new InstanceCommand()
            //            {
            //                CommandAction = CommandAction.SetRaidLeader,
            //                CommandParam1 = RaidLeader.CharacterName,
            //            };

            //            _socketServer.SendCommandToProcess(characterState.ProcessId, setLeaderCommand);
            //            return;

            //        }
            //        else if (RaidLeader.CharacterName != characterState.CharacterName && !characterState.InParty)
            //        {
            //            InstanceCommand addPartyMember = new InstanceCommand()
            //            {
            //                CommandAction = CommandAction.AddPartyMember,
            //                CommandParam1 = characterState.CharacterName,
            //            };

            //            _socketServer.SendCommandToProcess(RaidLeader.ProcessId, addPartyMember);
            //            return;
            //        }
            //        else if (PartyMembers.All(x => x.InParty))
            //        {
            //            if (PartyMembers.All(x => x.MapId == _ACTIVITY_TELE_IDS[Activity].Item1))
            //            {

            //                InstanceCommand beginDungeon = new InstanceCommand()
            //                {
            //                    CommandAction = CommandAction.BeginDungeon,
            //                };

            //                _socketServer.SendCommandToProcess(characterState.ProcessId, beginDungeon);
            //                return;
            //            }
            //            else
            //            {
            //                if (characterState.MapId != _ACTIVITY_TELE_IDS[Activity].Item1)
            //                {
            //                    AreaTriggerTeleport areaTriggerTeleport = SqliteRepository.GetAreaTriggerTeleportById(_ACTIVITY_TELE_IDS[Activity].Item2);
            //                    InstanceCommand goToCommand = new InstanceCommand()
            //                    {
            //                        CommandAction = CommandAction.GoTo,
            //                        CommandParam1 = areaTriggerTeleport.TargetPositionX.ToString(),
            //                        CommandParam2 = areaTriggerTeleport.TargetPositionY.ToString(),
            //                        CommandParam3 = areaTriggerTeleport.TargetPositionZ.ToString(),
            //                    };

            //                    _socketServer.SendCommandToProcess(characterState.ProcessId, goToCommand);
            //                    return;
            //                }
            //            }
            //        }
            //    }
            //}
            _socketServer.SendCommandToProcess(characterState.ProcessId, new InstanceCommand());
        }
    }
}
