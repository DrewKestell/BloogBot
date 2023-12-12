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
using RaidMemberBot.Game;
using static RaidMemberBot.Constants.Enums;

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
                        else if (FindMissingSkills(raidMemberViewModel.Skills, newCharacterState, out int skillSpellId))
                        {
                            InstanceCommand addSpell = new InstanceCommand()
                            {
                                CommandAction = CommandAction.AddSpell,
                                CommandParam1 = skillSpellId.ToString()
                            };

                            NextCommand[newCharacterState.ProcessId] = addSpell;
                        }
                        else if (FindMissingSpells(raidMemberViewModel.Spells, newCharacterState, out int spellId))
                        {
                            InstanceCommand addSpell = new InstanceCommand()
                            {
                                CommandAction = CommandAction.AddSpell,
                                CommandParam1 = spellId.ToString()
                            };

                            NextCommand[newCharacterState.ProcessId] = addSpell;
                        }
                        else if (RaidLeader.Guid != newCharacterState.Guid && !newCharacterState.InParty)
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
                            if (PartyMembersToStates.Any(x => x.Value.MapId != MapId))
                            {
                                if (newCharacterState.Level != raidMemberViewModel.RaidMemberPreset.Level)
                                {
                                    InstanceCommand setLevelCommand = new InstanceCommand()
                                    {
                                        CommandAction = CommandAction.SetLevel,
                                        CommandParam1 = raidMemberViewModel.RaidMemberPreset.Level.ToString()
                                    };
                                    NextCommand[newCharacterState.ProcessId] = setLevelCommand;
                                }
                                else if (newCharacterState.MapId != MapId)
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
                                if (newCharacterState.Task == "Idle")
                                {
                                    InstanceCommand beginDungeon = new InstanceCommand()
                                    {
                                        CommandAction = CommandAction.BeginDungeon,
                                    };

                                    NextCommand[newCharacterState.ProcessId] = beginDungeon;
                                }
                                else
                                {
                                    InstanceCommand noneCommand = new InstanceCommand()
                                    {
                                        CommandAction = CommandAction.None,
                                    };

                                    NextCommand[newCharacterState.ProcessId] = noneCommand;
                                }
                            }
                        }
                    }
                }
            }
            SendCommandToProcess(newCharacterState.ProcessId, NextCommand[newCharacterState.ProcessId]);
        }

        private bool FindMissingSkills(List<int> skills, CharacterState newCharacterState, out int skillSpellId)
        {
            skillSpellId = 0;
            for (int i = 0; i < skills.Count; i++)
            {
                if (!newCharacterState.SkillList.Contains(skills[i]))
                {
                    skillSpellId = SkillsToSpellsList[skills[i]];
                    Console.WriteLine($"[ACTIVITY MANAGER]{newCharacterState.CharacterName} Add skill {skills[i]} with spell {skillSpellId} {JsonConvert.SerializeObject(newCharacterState.SkillList)} {JsonConvert.SerializeObject(newCharacterState.SpellList)}");
                    return true;
                }
            }
            return false;
        }

        private bool FindMissingSpells(List<int> spellList, CharacterState newCharacterState, out int spellId)
        {
            spellId = 0;

            for (int i = 0; i < spellList.Count; i++)
            {
                if (!newCharacterState.SpellList.Contains(spellList[i]))
                {
                    Console.WriteLine($"[ACTIVITY MANAGER]{newCharacterState.CharacterName} Add spell {spellList[i]}");
                    spellId = spellList[i];
                    return true;
                }
            }
            return false;
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

        public Dictionary<int, int> SkillsToSpellsList = new Dictionary<int, int>()
        {
            { 40, 2842 },
            { 43, 201 },
            { 44, 196 },
            { 45, 264 },
            { 46, 266 },
            { 54, 198 },
            { 55, 202 },
            { 118, 674 },
            { 129, 3273 },
            { 136, 227 },
            { 160, 199 },
            //{ 164, 2 },
            //{ 165, 2 },
            //{ 171, 2 },
            { 172, 197 },
            { 173, 1180 },
            { 176, 2567 },
            //{ 182, 2 },
            //{ 185, 2 },
            //{ 186, 2 },
            //{ 197, 2 },
            //{ 202, 2 },
            { 226, 5011 },
            { 228, 5009 },
            { 229, 200 },
            //{ 293, 2 },
            //{ 333, 2 },
            //{ 356, 2 },
            //{ 393, 2 },
            { 413, 8737 },
            { 414, 9077 },
            { 415, 9078 },
            { 433, 9116 },
            //{ 473, 15590 },
            { 633, 1804 },
        };
    }
}
