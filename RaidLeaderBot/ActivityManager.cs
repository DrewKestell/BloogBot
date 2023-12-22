using System.Diagnostics;
using System.Collections.Generic;
using static RaidLeaderBot.WinImports;
using System;
using System.Linq;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Models;
using System.Net;
using System.Windows;
using static RaidMemberBot.Constants.Enums;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RaidLeaderBot
{
    public class ActivityManager
    {
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
            Application.Current.Dispatcher.Invoke(() => { CheckForCommand(UpdateCharacterState(state), state); });
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
                            characterState = new CharacterState();
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
                    if (!newCharacterState.IsReset)
                    {
                        if (newCharacterState.AccountName != raidMemberViewModel.AccountName
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
                            InstanceCommand resetCharacterState = new InstanceCommand()
                            {
                                CommandAction = CommandAction.ResetCharacterState,
                            };

                            NextCommand[newCharacterState.ProcessId] = resetCharacterState;
                        }
                    }
                    else if (newCharacterState.IsReset && !newCharacterState.IsReadyToStart)
                    {
                        if (newCharacterState.Zone != "GM Island")
                        {
                            InstanceCommand goToCommand = new InstanceCommand()
                            {
                                CommandAction = CommandAction.TeleTo,
                                CommandParam1 = "16226",
                                CommandParam2 = "16257",
                                CommandParam3 = "13",
                                CommandParam4 = "1",
                            };
                            NextCommand[newCharacterState.ProcessId] = goToCommand;
                        }
                        else if (RaidLeader.CharacterName != newCharacterState.RaidLeader)
                        {
                            InstanceCommand setLeaderCommand = new InstanceCommand()
                            {
                                CommandAction = CommandAction.SetRaidLeader,
                                CommandParam1 = RaidLeader.CharacterName,
                                CommandParam2 = RaidLeader.Guid.ToString()
                            };

                            NextCommand[newCharacterState.ProcessId] = setLeaderCommand;
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
                        else if (newCharacterState.Level < raidMemberViewModel.Level)
                        {
                            InstanceCommand setLevelCommand = new InstanceCommand()
                            {
                                CommandAction = CommandAction.SetLevel,
                                CommandParam1 = raidMemberViewModel.Level.ToString()
                            };

                            NextCommand[newCharacterState.ProcessId] = setLevelCommand;
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
                        else if (FindMissingEquipment(raidMemberViewModel, newCharacterState, out int itemId, out EquipSlot equipSlot))
                        {
                            InstanceCommand addEquipment = new InstanceCommand()
                            {
                                CommandAction = CommandAction.AddEquipment,
                                CommandParam1 = itemId.ToString(),
                                CommandParam2 = ((int)equipSlot).ToString()
                            };

                            NextCommand[newCharacterState.ProcessId] = addEquipment;
                        }
                        else if (FindMissingTalents(raidMemberViewModel, newCharacterState, out int preTalentSpellId))
                        {
                            InstanceCommand addTalent = new InstanceCommand()
                            {
                                CommandAction = CommandAction.AddTalent,
                                CommandParam1 = preTalentSpellId.ToString(),
                            };

                            NextCommand[newCharacterState.ProcessId] = addTalent;
                        }
                        else if (!newCharacterState.IsReadyToStart)
                        {
                            InstanceCommand setReadyState = new InstanceCommand()
                            {
                                CommandAction = CommandAction.SetReadyState,
                                CommandParam1 = true.ToString()
                            };

                            NextCommand[newCharacterState.ProcessId] = setReadyState;
                        }
                    }
                    else if (PartyMembersToStates.All(x => x.Value.InParty && x.Value.IsReadyToStart))
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
                            if (newCharacterState.Task == "Idle")
                            {
                                InstanceCommand beginDungeon = new InstanceCommand()
                                {
                                    CommandAction = CommandAction.BeginDungeon,
                                };

                                NextCommand[newCharacterState.ProcessId] = beginDungeon;
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
            SendCommandToProcess(newCharacterState.ProcessId, NextCommand[newCharacterState.ProcessId]);
        }

        private bool FindMissingTalents(RaidMemberViewModel raidMemberViewModel, CharacterState newCharacterState, out int talentSpellId)
        {
            talentSpellId = 0;
            if (raidMemberViewModel.Talents.Count > 0 && newCharacterState.Level > 9)
            {
                int max = Math.Min(newCharacterState.Level - 9, raidMemberViewModel.Talents.Count);
                for (int i = 0; i < max; i++)
                {
                    if (!newCharacterState.Talents.Contains(raidMemberViewModel.Talents[i]))
                    {
                        talentSpellId = raidMemberViewModel.Talents[i];
                        return true;
                    }
                }
            }
            return false;
        }

        private bool FindMissingEquipment(RaidMemberViewModel raidMemberViewModel, CharacterState newCharacterState, out int itemId, out EquipSlot inventorySlot)
        {
            itemId = 0;
            inventorySlot = EquipSlot.Ammo;

            if (newCharacterState.HeadItem != raidMemberViewModel.HeadItem.Entry)
            {
                inventorySlot = EquipSlot.Head;
                itemId = raidMemberViewModel.HeadItem.Entry;
                return true;
            }
            else if (newCharacterState.NeckItem != raidMemberViewModel.NeckItem.Entry)
            {
                inventorySlot = EquipSlot.Neck;
                itemId = raidMemberViewModel.NeckItem.Entry;
                return true;
            }
            else if (newCharacterState.ShoulderItem != raidMemberViewModel.ShoulderItem.Entry)
            {
                inventorySlot = EquipSlot.Shoulders;
                itemId = raidMemberViewModel.ShoulderItem.Entry;
                return true;
            }
            else if (newCharacterState.BackItem != raidMemberViewModel.BackItem.Entry)
            {
                inventorySlot = EquipSlot.Back;
                itemId = raidMemberViewModel.BackItem.Entry;
                return true;
            }
            else if (newCharacterState.ChestItem != raidMemberViewModel.ChestItem.Entry)
            {
                inventorySlot = EquipSlot.Chest;
                itemId = raidMemberViewModel.ChestItem.Entry;
                return true;
            }
            else if (newCharacterState.ShirtItem != raidMemberViewModel.ShirtItem.Entry)
            {
                inventorySlot = EquipSlot.Shirt;
                itemId = raidMemberViewModel.ShirtItem.Entry;
                return true;
            }
            else if (newCharacterState.Tabardtem != raidMemberViewModel.TabardItem.Entry)
            {
                inventorySlot = EquipSlot.Tabard;
                itemId = raidMemberViewModel.TabardItem.Entry;
                return true;
            }
            else if (newCharacterState.WristsItem != raidMemberViewModel.WristsItem.Entry)
            {
                inventorySlot = EquipSlot.Wrist;
                itemId = raidMemberViewModel.WristsItem.Entry;
                return true;
            }
            else if (newCharacterState.HandsItem != raidMemberViewModel.HandsItem.Entry)
            {
                inventorySlot = EquipSlot.Hands;
                itemId = raidMemberViewModel.HandsItem.Entry;
                return true;
            }
            else if (newCharacterState.WaistItem != raidMemberViewModel.WaistItem.Entry)
            {
                inventorySlot = EquipSlot.Waist;
                itemId = raidMemberViewModel.WaistItem.Entry;
                return true;
            }
            else if (newCharacterState.LegsItem != raidMemberViewModel.LegsItem.Entry)
            {
                inventorySlot = EquipSlot.Legs;
                itemId = raidMemberViewModel.LegsItem.Entry;
                return true;
            }
            else if (newCharacterState.FeetItem != raidMemberViewModel.FeetItem.Entry)
            {
                inventorySlot = EquipSlot.Feet;
                itemId = raidMemberViewModel.FeetItem.Entry;
                return true;
            }
            else if (newCharacterState.Finger1Item != raidMemberViewModel.Finger1Item.Entry)
            {
                inventorySlot = EquipSlot.Finger1;
                itemId = raidMemberViewModel.Finger1Item.Entry;
                return true;
            }
            else if (newCharacterState.Finger2Item != raidMemberViewModel.Finger2Item.Entry)
            {
                inventorySlot = EquipSlot.Finger2;
                itemId = raidMemberViewModel.Finger2Item.Entry;
                return true;
            }
            else if (newCharacterState.Trinket1Item != raidMemberViewModel.Trinket1Item.Entry)
            {
                inventorySlot = EquipSlot.Trinket1;
                itemId = raidMemberViewModel.Trinket1Item.Entry;
                return true;
            }
            else if (newCharacterState.Trinket2Item != raidMemberViewModel.Trinket2Item.Entry)
            {
                inventorySlot = EquipSlot.Trinket2;
                itemId = raidMemberViewModel.Trinket2Item.Entry;
                return true;
            }
            else if (newCharacterState.MainHandItem != raidMemberViewModel.MainHandItem.Entry)
            {
                inventorySlot = EquipSlot.MainHand;
                itemId = raidMemberViewModel.MainHandItem.Entry;
                return true;
            }
            else if (newCharacterState.OffHandItem != raidMemberViewModel.OffHandItem.Entry)
            {
                inventorySlot = EquipSlot.OffHand;
                itemId = raidMemberViewModel.OffHandItem.Entry;
                return true;
            }
            else if (newCharacterState.RangedItem != raidMemberViewModel.RangedItem.Entry)
            {
                inventorySlot = EquipSlot.Ranged;
                itemId = raidMemberViewModel.RangedItem.Entry;
                return true;
            }
            return false;
        }

        private bool FindMissingSkills(List<int> skills, CharacterState newCharacterState, out int skillSpellId)
        {
            skillSpellId = 0;
            for (int i = 0; i < skills.Count; i++)
            {
                if (!newCharacterState.Skills.Contains(skills[i]))
                {
                    skillSpellId = SkillsToSpellsList[skills[i]];
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
                if (!newCharacterState.Spells.Contains(spellList[i]))
                {
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
