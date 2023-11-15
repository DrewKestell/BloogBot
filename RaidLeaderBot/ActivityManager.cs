using System.Diagnostics;
using System.Collections.Generic;
using static RaidLeaderBot.WinImports;
using System;
using System.Linq;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Models;
using System.Net;
using System.Windows;
using Newtonsoft.Json;

namespace RaidLeaderBot
{
    public class ActivityManager
    {
        public object _lock = new object();
        public ActivityType Activity { get; }
        public CharacterState RaidLeader { set; get; }
        public int RaidSize { get; }
        public int MapId { get; }
        public int HostMapId { get; }
        public CommandSocketServer _commandSocketServer { get; }

        public readonly Dictionary<RaidMemberViewModel, CharacterState> PartyMembersToStates = new Dictionary<RaidMemberViewModel, CharacterState>();
        private readonly Dictionary<int, InstanceCommand> NextCommand = new Dictionary<int, InstanceCommand>();

        public ActivityManager(int portNumber, int mapId)
        {
            _commandSocketServer = new CommandSocketServer(portNumber, IPAddress.Parse(RaidLeaderBotSettings.Instance.ListenAddress));
            _commandSocketServer.Start();

            _commandSocketServer.InstanceUpdateObservable.Subscribe(OnInstanceUpdate);

            MapId = mapId;
        }
        ~ActivityManager()
        {
            _commandSocketServer?.Stop();
        }

        private void OnInstanceUpdate(CharacterState state)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CheckForCommand(UpdateCharacterState(state), state);
            });
        }

        private RaidMemberViewModel UpdateCharacterState(CharacterState characterState)
        {
            if (PartyMembersToStates.Values.Any(x => x.ProcessId == characterState.ProcessId))
            {
                for (int i = 0; i < PartyMembersToStates.Values.Count; i++)
                {
                    if (PartyMembersToStates.Values.ElementAt(i).ProcessId == characterState.ProcessId)
                    {
                        PartyMembersToStates[PartyMembersToStates.Keys.ElementAt(i)] = characterState;

                        if (!characterState.IsConnected)
                        {
                            characterState.ProcessId = 0;
                        }
                        else
                        {
                            SetWindowText(Process.GetProcessById(characterState.ProcessId).MainWindowHandle, $"WoW - Player {i + 1}");
                        }

                        return PartyMembersToStates.Keys.ElementAt(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < PartyMembersToStates.Count; i++)
                {
                    if (PartyMembersToStates.Values.ElementAt(i).ProcessId == 0)
                    {
                        PartyMembersToStates[PartyMembersToStates.Keys.ElementAt(i)] = characterState;
                        NextCommand.Add(characterState.ProcessId, new InstanceCommand());
                        return PartyMembersToStates.Keys.ElementAt(i);
                    }
                }
            }
            return null;
        }
        private void CheckForCommand(RaidMemberViewModel raidMemberViewModel, CharacterState newCharacterState)
        {
            if (raidMemberViewModel.ShouldRun)
            {
                if (newCharacterState.ProcessId > 0)
                {
                    if (newCharacterState.Guid == 0
                        || newCharacterState.AccountName != raidMemberViewModel.AccountName
                        || newCharacterState.BotProfileName != raidMemberViewModel.BotProfileName)
                    {
                        InstanceCommand loginCommand = new InstanceCommand()
                        {
                            CommandAction = CommandAction.SetAccountInfo,
                            CommandParam1 = raidMemberViewModel.AccountName,
                            CommandParam2 = raidMemberViewModel.BotProfileName,
                        };

                        NextCommand[newCharacterState.ProcessId] = loginCommand;
                    }
                    else if (string.IsNullOrEmpty(newCharacterState.CurrentActivity))
                    {
                        InstanceCommand setActivityCommand = new InstanceCommand()
                        {
                            CommandAction = CommandAction.SetActivity,
                            CommandParam1 = Activity.ToString(),
                        };

                        NextCommand[newCharacterState.ProcessId] = setActivityCommand;
                    }
                    else if (RaidLeader == null)
                    {
                        if (raidMemberViewModel.RaidMemberPreset.IsMainTank && !string.IsNullOrEmpty(newCharacterState.CharacterName))
                        {
                            RaidLeader = newCharacterState;

                            InstanceCommand setLeaderCommand = new InstanceCommand()
                            {
                                CommandAction = CommandAction.SetRaidLeader,
                                CommandParam1 = RaidLeader.CharacterName,
                            };

                            NextCommand[newCharacterState.ProcessId] = setLeaderCommand;
                        }
                    }
                    else
                    {
                        if (RaidLeader.CharacterName != newCharacterState.RaidLeader)
                        {
                            InstanceCommand setLeaderCommand = new InstanceCommand()
                            {
                                CommandAction = CommandAction.SetRaidLeader,
                                CommandParam1 = RaidLeader.CharacterName,
                            };

                            NextCommand[newCharacterState.ProcessId] = setLeaderCommand;
                        }
                        else if (RaidLeader.CharacterName != newCharacterState.CharacterName && !newCharacterState.InParty)
                        {
                            InstanceCommand addPartyMember = new InstanceCommand()
                            {
                                CommandAction = CommandAction.AddPartyMember,
                                CommandParam1 = newCharacterState.CharacterName,
                            };

                            NextCommand[RaidLeader.ProcessId] = addPartyMember;
                        }
                        else if (PartyMembersToStates.All(x => x.Value.InParty))
                        {
                            if (PartyMembersToStates.All(x => x.Value.MapId == MapId))
                            {
                                InstanceCommand beginDungeon = new InstanceCommand()
                                {
                                    CommandAction = CommandAction.BeginDungeon,
                                };

                                NextCommand[newCharacterState.ProcessId] = beginDungeon;
                            }
                            else
                            {
                                if (newCharacterState.MapId != MapId)
                                {
                                    AreaTriggerTeleport areaTriggerTeleport = SqliteRepository.GetAreaTriggerTeleportByTargetMap(MapId);
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
                        }
                    }
                }
            }
            SendCommandToProcess(newCharacterState.ProcessId, NextCommand[newCharacterState.ProcessId]);
        }
        public void QueueCommandToProcess(int processId, InstanceCommand command)
        {
            NextCommand[processId] = command;
        }
        private void SendCommandToProcess(int processId, InstanceCommand command)
        {
            _commandSocketServer.SendCommandToProcess(processId, command);
            NextCommand[processId] = new InstanceCommand();
        }
        public void AddRaidMember(RaidMemberViewModel raidMemberViewModel)
        {
            PartyMembersToStates.Add(raidMemberViewModel, new CharacterState());
        }
        public void RemoveRaidMember(RaidMemberViewModel raidMemberViewModel)
        {
            PartyMembersToStates.Remove(raidMemberViewModel);
        }
    }
}
