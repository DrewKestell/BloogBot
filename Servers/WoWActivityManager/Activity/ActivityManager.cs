using System.Net;
using WoWActivityManager.Client;
using WoWActivityManager.Models;
using WoWClientBot.Models;
using static WoWClientBot.Constants.Enums;

namespace WoWActivityManager.Activity
{
    public abstract class ActivityManager
    {
        protected ActivityPreset Preset { get; }
        protected ActivityType Activity { get; }
        protected CharacterState RaidLeader { set; get; }
        protected virtual bool ActivityRunCondition { get; }
        protected ActivityManagerSocketServer _activitySocketServer { get; }
        protected WoWStateManagerClient WoWStateManagerClient { get; }

        public readonly Dictionary<ActivityMemberPreset, CharacterState> PartyMembersToStates = [];
        public readonly Dictionary<ActivityMemberPreset, CharacterState> PartyMembersToPreferredStates = [];
        protected readonly Dictionary<int, CharacterCommand> NextCommand = [];

        public ActivityManager(ActivityPreset activityPreset)
        {
            //Preset = activityPreset;

            //WoWStateManagerClient = new WoWStateManagerClient(Preset.WorldStateManagerPort, IPAddress.Parse(Preset.WorldStateManagerIPAddress));

            //_activitySocketServer = new ActivityManagerSocketServer(activityPreset.ActivityManagerPort, IPAddress.Parse("127.0.0.1"));
            //_activitySocketServer.Start();

            //_activitySocketServer.InstanceUpdateObservable.Subscribe(OnInstanceUpdate);
        }
        ~ActivityManager()
        {
            _activitySocketServer?.Stop();
        }

        protected void OnInstanceUpdate(CharacterState state)
        {
            ActivityMemberPreset activityMemberViewModel = UpdateCharacterState(state);
            if (PartyMembersToPreferredStates[activityMemberViewModel].ShouldRun)
                if (BotPreparedToStart(PartyMembersToPreferredStates[activityMemberViewModel], state))
                    CheckForCommand(activityMemberViewModel, state);
                else
                    NextCommand[state.ProcessId] = new CharacterCommand()
                    {
                        CharacterAction = CharacterAction.FullStop
                    };

            SendCommandToProcess(state.ProcessId, NextCommand[state.ProcessId]);
        }

        private ActivityMemberPreset UpdateCharacterState(CharacterState characterState)
        {
            if (PartyMembersToStates.Values.Any(x => x.ProcessId == characterState.ProcessId))
            {
                for (int i = 0; i < PartyMembersToStates.Values.Count; i++)
                {
                    if (PartyMembersToStates.Values.ElementAt(i).ProcessId == characterState.ProcessId)
                    {
                        PartyMembersToStates[PartyMembersToStates.Keys.ElementAt(i)] = characterState;
                        if (RaidLeader != null && characterState.RaidLeaderGuid == characterState.Guid)
                        {
                            RaidLeader = characterState;
                        }

                        if (!characterState.IsConnected)
                            characterState = new CharacterState();

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
                        NextCommand.Add(characterState.ProcessId, new CharacterCommand());
                        return PartyMembersToStates.Keys.ElementAt(i);
                    }
                }
            }
            return null;
        }
        protected abstract void CheckForCommand(ActivityMemberPreset activityMemberPreset, CharacterState newCharacterState);
        protected bool BotPreparedToStart(CharacterState preferredState, CharacterState characterState)
        {
            if (characterState.ProcessId > 0)
            {
                if (!characterState.IsReset)
                {
                    if (characterState.AccountName != preferredState.AccountName || characterState.BotProfileName != preferredState.BotProfileName)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.SetAccountInfo,
                            CommandParam1 = preferredState.AccountName,
                            CommandParam2 = preferredState.BotProfileName,
                        };
                    }
                    else if (characterState.Zone != "GM Island")
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.TeleTo,
                            CommandParam1 = "16226",
                            CommandParam2 = "16257",
                            CommandParam3 = "13",
                            CommandParam4 = "1",
                        };
                    }
                    else if (string.IsNullOrEmpty(characterState.CurrentActivity))
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.SetActivity,
                            CommandParam1 = Activity.ToString(),
                            //CommandParam2 = Preset.MapId.ToString(),
                        };
                    }
                    else if (RaidLeader == null)
                    {
                        if (preferredState.CharacterConfig.IsMainTank && !string.IsNullOrEmpty(characterState.CharacterName))
                        {
                            RaidLeader = characterState;

                            NextCommand[characterState.ProcessId] = new CharacterCommand()
                            {
                                CharacterAction = CharacterAction.SetRaidLeader,
                                CommandParam1 = RaidLeader.CharacterName,
                                CommandParam2 = RaidLeader.Guid.ToString(),
                            };
                        }
                    }
                    else
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.ResetCharacterState,
                        };
                    }
                }
                else if (!characterState.IsReadyToStart)
                {
                    if (characterState.Level < preferredState.CharacterConfig.Level)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.SetLevel,
                            CommandParam1 = preferredState.CharacterConfig.Level.ToString()
                        };
                    }
                    else if (FindMissingSkills(preferredState.CharacterConfig.Skills, characterState, out int skillSpellId))
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddSpell,
                            CommandParam1 = skillSpellId.ToString()
                        };
                    }
                    else if (FindMissingSpells(preferredState.CharacterConfig.Spells, characterState, out int spellId))
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddSpell,
                            CommandParam1 = spellId.ToString()
                        };
                    }
                    else if (FindMissingEquipment(preferredState, characterState, out int itemId, out EquipSlot equipSlot))
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddEquipment,
                            CommandParam1 = itemId.ToString(),
                            CommandParam2 = ((int)equipSlot).ToString()
                        };
                    }
                    else if (FindMissingTalents(preferredState, characterState, out int preTalentSpellId))
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddTalent,
                            CommandParam1 = preTalentSpellId.ToString(),
                        };
                    }
                    else if (preferredState.CharacterConfig.IsRole1 && !characterState.CharacterConfig.IsRole1)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddRole,
                            CommandParam1 = "1",
                        };
                    }
                    else if (preferredState.CharacterConfig.IsRole2 && !characterState.CharacterConfig.IsRole2)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddRole,
                            CommandParam1 = "2",
                        };
                    }
                    else if (preferredState.CharacterConfig.IsRole3 && !characterState.CharacterConfig.IsRole3)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddRole,
                            CommandParam1 = "3",
                        };
                    }
                    else if (preferredState.CharacterConfig.IsRole4 && !characterState.CharacterConfig.IsRole4)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddRole,
                            CommandParam1 = "4",
                        };
                    }
                    else if (preferredState.CharacterConfig.IsRole5 && !characterState.CharacterConfig.IsRole5)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddRole,
                            CommandParam1 = "5",
                        };
                    }
                    else if (preferredState.CharacterConfig.IsRole6 && !characterState.CharacterConfig.IsRole6)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddRole,
                            CommandParam1 = "6",
                        };
                    }
                    else if (preferredState.CharacterConfig.IsMainTank && !characterState.CharacterConfig.IsMainTank)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddRole,
                            CommandParam1 = "7",
                        };
                    }
                    else if (preferredState.CharacterConfig.IsMainHealer && !characterState.CharacterConfig.IsMainHealer)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddRole,
                            CommandParam1 = "8",
                        };
                    }
                    else if (preferredState.CharacterConfig.IsOffTank && !characterState.CharacterConfig.IsOffTank)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddRole,
                            CommandParam1 = "9",
                        };
                    }
                    else if (preferredState.CharacterConfig.IsOffHealer && !characterState.CharacterConfig.IsOffHealer)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddRole,
                            CommandParam1 = "10",
                        };
                    }
                    else if (preferredState.CharacterConfig.ShouldCleanse && !characterState.CharacterConfig.ShouldCleanse)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddRole,
                            CommandParam1 = "11",
                        };
                    }
                    else if (preferredState.CharacterConfig.ShouldRebuff && !characterState.CharacterConfig.ShouldRebuff)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.AddRole,
                            CommandParam1 = "12",
                        };
                    }
                    else if (RaidLeader.Guid != characterState.RaidLeaderGuid)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.SetRaidLeader,
                            CommandParam1 = RaidLeader.CharacterName,
                            CommandParam2 = RaidLeader.Guid.ToString()
                        };
                    }
                    else if (!characterState.IsReadyToStart)
                    {
                        NextCommand[characterState.ProcessId] = new CharacterCommand()
                        {
                            CharacterAction = CharacterAction.SetReadyState,
                            CommandParam1 = true.ToString()
                        };
                    }
                }
                else if (RaidLeader.Guid != characterState.Guid && !(characterState.InParty || characterState.InRaid))
                {
                    NextCommand[RaidLeader.ProcessId] = new CharacterCommand()
                    {
                        CharacterAction = CharacterAction.AddPartyMember,
                        CommandParam1 = characterState.CharacterName,
                    };
                    return false;
                }
                else
                    return true;
            }
            return false;
        }

        protected bool FindMissingTalents(CharacterState preferredState, CharacterState newCharacterState, out int talentSpellId)
        {
            talentSpellId = 0;
            if (preferredState.CharacterConfig.Talents.Count > 0 && newCharacterState.Level > 9)
            {
                int max = Math.Min(newCharacterState.Level - 9, preferredState.CharacterConfig.Talents.Count);
                for (int i = 0; i < max; i++)
                {
                    if (!newCharacterState.CharacterConfig.Talents.Contains(preferredState.CharacterConfig.Talents[i]))
                    {
                        talentSpellId = preferredState.CharacterConfig.Talents[i];
                        return true;
                    }
                }
            }
            return false;
        }

        protected bool FindMissingEquipment(CharacterState preferredState, CharacterState newCharacterState, out int itemId, out EquipSlot inventorySlot)
        {
            itemId = 0;
            inventorySlot = EquipSlot.Ammo;

            if (newCharacterState.CharacterConfig.HeadItem != preferredState.CharacterConfig.HeadItem)
            {
                inventorySlot = EquipSlot.Head;
                itemId = preferredState.CharacterConfig.HeadItem;
                return true;
            }
            else if (newCharacterState.CharacterConfig.NeckItem != preferredState.CharacterConfig.NeckItem)
            {
                inventorySlot = EquipSlot.Neck;
                itemId = preferredState.CharacterConfig.NeckItem;
                return true;
            }
            else if (newCharacterState.CharacterConfig.ShoulderItem != preferredState.CharacterConfig.ShoulderItem)
            {
                inventorySlot = EquipSlot.Shoulders;
                itemId = preferredState.CharacterConfig.ShoulderItem;
                return true;
            }
            else if (newCharacterState.CharacterConfig.BackItem != preferredState.CharacterConfig.BackItem)
            {
                inventorySlot = EquipSlot.Back;
                itemId = preferredState.CharacterConfig.BackItem;
                return true;
            }
            else if (newCharacterState.CharacterConfig.ChestItem != preferredState.CharacterConfig.ChestItem)
            {
                inventorySlot = EquipSlot.Chest;
                itemId = preferredState.CharacterConfig.ChestItem;
                return true;
            }
            else if (newCharacterState.CharacterConfig.ShirtItem != preferredState.CharacterConfig.ShirtItem)
            {
                inventorySlot = EquipSlot.Shirt;
                itemId = preferredState.CharacterConfig.ShirtItem;
                return true;
            }
            else if (newCharacterState.CharacterConfig.TabardItem != preferredState.CharacterConfig.TabardItem)
            {
                inventorySlot = EquipSlot.Tabard;
                itemId = preferredState.CharacterConfig.TabardItem;
                return true;
            }
            else if (newCharacterState.CharacterConfig.WristsItem != preferredState.CharacterConfig.WristsItem)
            {
                inventorySlot = EquipSlot.Wrist;
                itemId = preferredState.CharacterConfig.WristsItem;
                return true;
            }
            else if (newCharacterState.CharacterConfig.HandsItem != preferredState.CharacterConfig.HandsItem)
            {
                inventorySlot = EquipSlot.Hands;
                itemId = preferredState.CharacterConfig.HandsItem;
                return true;
            }
            else if (newCharacterState.CharacterConfig.WaistItem != preferredState.CharacterConfig.WaistItem)
            {
                inventorySlot = EquipSlot.Waist;
                itemId = preferredState.CharacterConfig.WaistItem;
                return true;
            }
            else if (newCharacterState.CharacterConfig.LegsItem != preferredState.CharacterConfig.LegsItem)
            {
                inventorySlot = EquipSlot.Legs;
                itemId = preferredState.CharacterConfig.LegsItem;
                return true;
            }
            else if (newCharacterState.CharacterConfig.FeetItem != preferredState.CharacterConfig.FeetItem)
            {
                inventorySlot = EquipSlot.Feet;
                itemId = preferredState.CharacterConfig.FeetItem;
                return true;
            }
            else if (newCharacterState.CharacterConfig.Finger1Item != preferredState.CharacterConfig.Finger1Item)
            {
                inventorySlot = EquipSlot.Finger1;
                itemId = preferredState.CharacterConfig.Finger1Item;
                return true;
            }
            else if (newCharacterState.CharacterConfig.Finger2Item != preferredState.CharacterConfig.Finger2Item)
            {
                inventorySlot = EquipSlot.Finger2;
                itemId = preferredState.CharacterConfig.Finger2Item;
                return true;
            }
            else if (newCharacterState.CharacterConfig.Trinket1Item != preferredState.CharacterConfig.Trinket1Item)
            {
                inventorySlot = EquipSlot.Trinket1;
                itemId = preferredState.CharacterConfig.Trinket1Item;
                return true;
            }
            else if (newCharacterState.CharacterConfig.Trinket2Item != preferredState.CharacterConfig.Trinket2Item)
            {
                inventorySlot = EquipSlot.Trinket2;
                itemId = preferredState.CharacterConfig.Trinket2Item;
                return true;
            }
            else if (newCharacterState.CharacterConfig.MainHandItem != preferredState.CharacterConfig.MainHandItem)
            {
                inventorySlot = EquipSlot.MainHand;
                itemId = preferredState.CharacterConfig.MainHandItem;
                return true;
            }
            else if (newCharacterState.CharacterConfig.OffHandItem != preferredState.CharacterConfig.OffHandItem)
            {
                inventorySlot = EquipSlot.OffHand;
                itemId = preferredState.CharacterConfig.OffHandItem;
                return true;
            }
            else if (newCharacterState.CharacterConfig.RangedItem != preferredState.CharacterConfig.RangedItem)
            {
                inventorySlot = EquipSlot.Ranged;
                itemId = preferredState.CharacterConfig.RangedItem;
                return true;
            }
            return false;
        }

        protected bool FindMissingSkills(List<int> skills, CharacterState newCharacterState, out int skillSpellId)
        {
            skillSpellId = 0;
            for (int i = 0; i < skills.Count; i++)
            {
                if (!newCharacterState.CharacterConfig.Skills.Contains(skills[i]))
                {
                    skillSpellId = SkillsToSpellsList[skills[i]];
                    return true;
                }
            }
            return false;
        }

        protected bool FindMissingSpells(List<int> spellList, CharacterState newCharacterState, out int spellId)
        {
            spellId = 0;

            for (int i = 0; i < spellList.Count; i++)
            {
                if (!newCharacterState.CharacterConfig.Spells.Contains(spellList[i]))
                {
                    spellId = spellList[i];
                    return true;
                }
            }
            return false;
        }

        public void QueueCommandToProcess(int processId, CharacterCommand command)
        {
            NextCommand[processId] = command;
        }
        protected void SendCommandToProcess(int processId, CharacterCommand command)
        {
            _activitySocketServer.SendCommandToProcess(processId, command);
            NextCommand[processId] = new CharacterCommand();
        }
        public void AddActivityMember(ActivityMemberPreset activityMemberPreset)
        {
            PartyMembersToStates.Add(activityMemberPreset, new CharacterState());
        }
        public void RemoveActivityMember(ActivityMemberPreset activityMemberPreset)
        {
            PartyMembersToStates.Remove(activityMemberPreset);
        }

        protected Dictionary<int, int> SkillsToSpellsList = new()
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
