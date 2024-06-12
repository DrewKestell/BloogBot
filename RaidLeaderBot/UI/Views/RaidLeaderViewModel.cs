using EnvDTE80;
using RaidLeaderBot.Activity;
using RaidLeaderBot.Utilities;
using RaidMemberBot.Models.Dto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;

namespace RaidLeaderBot
{
    public sealed class RaidLeaderViewModel : INotifyPropertyChanged, IDisposable
    {
        private ActivityManager _activityContainer;
        private readonly RaidPreset _preset;
        private readonly ProcessTracker _processTracker;
        public int Index { get; set; }
        public IEnumerable<ActivityType> EnumActivityTypes => Enum.GetValues(typeof(ActivityType)).Cast<ActivityType>();
        public ObservableCollection<RaidMemberViewModel> RaidMemberViewModels { get; set; } = new ObservableCollection<RaidMemberViewModel>();
        public RaidLeaderViewModel()
        {
            _preset = new RaidPreset();
            _processTracker = new ProcessTracker();
        }
        public RaidLeaderViewModel(RaidPreset raidPreset)
        {
            _preset = raidPreset;
            _processTracker = new ProcessTracker();
            _preset.RaidMemberPresets ??= new List<RaidMemberPreset>() { new RaidMemberPreset() };

            SetCurrentActivty();

            for (int i = 0; i < _preset.RaidMemberPresets.Count; i++)
            {
                AddRaidMember(new RaidMemberViewModel(_preset.RaidMemberPresets[i])
                {
                    Index = i,
                    IsAlliance = IsAlliance
                });
            }

            OnPropertyChanged(nameof(CurrentActivity));
            OnPropertyChanged(nameof(IsAlliance));
            OnPropertyChanged(nameof(IsHorde));
        }

        public CharacterState GetCharacterStateByRaidMemberViewModel(RaidMemberViewModel index) => _activityContainer.PartyMembersToStates[index];

        private ICommand _startAllRaidMembersCommand;
        private ICommand _stopAllRaidMembersCommand;
        private ICommand _setMaxRaidSizeCommand;

        private ICommand _addRaidMemberCommand;
        private ICommand _removeRaidMemberCommand;

        public ICommand StartAllRaidMembersCommand => _startAllRaidMembersCommand ??= new CommandHandler(StartAllRaidMembers, true);
        public ICommand StopAllRaidMembersCommand => _stopAllRaidMembersCommand ??= new CommandHandler(StopAllRaidMembers, true);
        public ICommand AddRaidMemberCommand => _addRaidMemberCommand ??= new CommandHandler(AddRaidMember, true);
        public ICommand RemoveRaidMemberCommand => _removeRaidMemberCommand ??= new CommandHandler(RemoveRaidMember, true);
        public ICommand SetMaxRaidSizeCommand => _setMaxRaidSizeCommand ??= new CommandHandler(SetMaxRaidSize, true);

        public bool CanAddMember => !ShouldRun && IsFocused && RaidMemberViewModels.Count < MaxGroupSize;
        public bool CanRemoveMember => !ShouldRun && IsFocused && RaidMemberViewModels.Count > 1;
        public bool CanStart => RaidMemberViewModels.Any(x => !x.ShouldRun);
        public bool CanStop => RaidMemberViewModels.Any(x => x.ShouldRun);
        public int MaxGroupSize
        {
            get
            {
                switch (CurrentActivity)
                {
                    case ActivityType.BlackrockDepths:
                    case ActivityType.DireMaul:
                    case ActivityType.Scholomance:
                    case ActivityType.StratholmeAlive:
                    case ActivityType.StratholmeUndead:
                        return 5;
                    case ActivityType.UpperBlackrockSpire:
                    case ActivityType.WarsongGulch19:
                    case ActivityType.WarsongGulch29:
                    case ActivityType.WarsongGulch39:
                    case ActivityType.WarsongGulch49:
                    case ActivityType.WarsongGulch59:
                    case ActivityType.WarsongGulch60:
                        return 10;
                    case ActivityType.ArathiBasin29:
                    case ActivityType.ArathiBasin39:
                    case ActivityType.ArathiBasin49:
                    case ActivityType.ArathiBasin59:
                    case ActivityType.ArathiBasin60:
                        return 15;
                    case ActivityType.ZulGurub:
                    case ActivityType.RuinsOfAhnQiraj:
                        return 20;
                    case ActivityType.Idle:
                    case ActivityType.AlteracValley:
                    case ActivityType.BlackwingLair:
                    case ActivityType.MoltenCore:
                    case ActivityType.Naxxramas:
                    case ActivityType.OnyxiasLair:
                    case ActivityType.TempleOfAhnQiraj:
                        return 40;
                }
                return 10;
            }
        }
        public int MinLevelRequirement
        {
            get
            {
                switch (CurrentActivity)
                {
                    case ActivityType.RagefireChasm:
                        return 8;
                    case ActivityType.WarsongGulch19:
                    case ActivityType.ShadowfangKeep:
                    case ActivityType.TheDeadmines:
                    case ActivityType.WailingCaverns:
                        return 10;
                    case ActivityType.TheStockade:
                        return 15;
                    case ActivityType.RazorfenKraul:
                        return 17;
                    case ActivityType.BlackfathomDeeps:
                        return 19;
                    case ActivityType.ArathiBasin29:
                    case ActivityType.WarsongGulch29:
                    case ActivityType.Gnomeregan:
                    case ActivityType.SMGraveyard:
                    case ActivityType.SMLibrary:
                    case ActivityType.SMArmory:
                    case ActivityType.SMCathedral:
                        return 20;
                    case ActivityType.RazorfenDowns:
                        return 25;
                    case ActivityType.ArathiBasin39:
                    case ActivityType.WarsongGulch39:
                    case ActivityType.Uldaman:
                    case ActivityType.MaraudonEarthSongFalls:
                    case ActivityType.MaraudonFoulsporeCavern:
                    case ActivityType.MaraudonWickedGrotto:
                        return 30;
                    case ActivityType.TempleOfAtalHakkar:
                    case ActivityType.ZulFarrak:
                        return 35;
                    case ActivityType.ArathiBasin49:
                    case ActivityType.WarsongGulch49:
                    case ActivityType.BlackrockDepths:
                        return 40;
                    case ActivityType.LowerBlackrockSpire:
                    case ActivityType.UpperBlackrockSpire:
                    case ActivityType.Scholomance:
                    case ActivityType.StratholmeAlive:
                    case ActivityType.StratholmeUndead:
                    case ActivityType.DireMaul:
                        return 45;
                    case ActivityType.ArathiBasin59:
                    case ActivityType.WarsongGulch59:
                    case ActivityType.MoltenCore:
                    case ActivityType.OnyxiasLair:
                    case ActivityType.ZulGurub:
                        return 50;
                    case ActivityType.AlteracValley:
                        return 51;
                    case ActivityType.ArathiBasin60:
                    case ActivityType.WarsongGulch60:
                    case ActivityType.BlackwingLair:
                    case ActivityType.RuinsOfAhnQiraj:
                    case ActivityType.TempleOfAhnQiraj:
                    case ActivityType.Naxxramas:
                        return 60;
                }
                return 1;
            }
        }
        public int MaxLevelRequirement
        {
            get
            {
                switch (CurrentActivity)
                {
                    case ActivityType.WarsongGulch19:
                        return 19;
                    case ActivityType.WarsongGulch29:
                    case ActivityType.ArathiBasin29:
                        return 29;
                    case ActivityType.WarsongGulch39:
                    case ActivityType.ArathiBasin39:
                        return 39;
                    case ActivityType.WarsongGulch49:
                    case ActivityType.ArathiBasin49:
                        return 49;
                    case ActivityType.WarsongGulch59:
                    case ActivityType.ArathiBasin59:
                        return 59;
                }
                return 60;
            }
        }

        public void StartAllRaidMembers()
        {
            try
            {
                var dte2 = (DTE2)Marshal.GetActiveObject("VisualStudio.DTE");
                var debugger = dte2.Debugger as Debugger2;
                for (int i = 0; i < RaidMemberViewModels.Count; i++)
                {
                    RaidMemberViewModels[i].ShouldRun = true;

                    if (_activityContainer.PartyMembersToStates[RaidMemberViewModels[i]].ProcessId == 0)
                    {
                        Dispatcher.CurrentDispatcher.InvokeAsync(async () =>
                        {
                            var processId = await RaidMemberLauncher.Instance.LaunchProcess(RaidLeaderPortNumber, _processTracker, CancellationToken.None);
                            if (processId != 0)
                            {
                                RetryWithTimeout retryWithTimeout = new RetryWithTimeout(() =>
                                {
                                    try
                                    {
                                        Process processToAttachTo = Process.GetProcessById(processId.Value);
                                        if (processToAttachTo != null)
                                        {
                                            foreach (EnvDTE.Process process in debugger.LocalProcesses)
                                            {
                                                if (process.ProcessID == processId)
                                                {
                                                    process.Attach();
                                                    Console.WriteLine($"Attached to process {processId}");
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                    catch (COMException ex) when (ex.HResult == unchecked((int)0x80010001)) // RPC_E_CALL_REJECTED
                                    {
                                        Console.WriteLine("Retrying due to COM exception...");
                                        Thread.Sleep(1000); // Wait before retrying
                                    }
                                    return false;
                                });
                                await retryWithTimeout.ExecuteWithRetry(CancellationToken.None);
                            }
                        });
                    }
                }

                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error encountered starting bot. {e.Message} {e.InnerException} {e.StackTrace}");
            }
        }
        public void StopAllRaidMembers()
        {
            try
            {
                for (int i = 0; i < RaidMemberViewModels.Count; i++)
                {
                    RaidMemberViewModels[i].ShouldRun = false;
                }

                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
            }
            catch
            {
                Console.WriteLine("Error encountered starting bot.");
            }
        }
        public void AddRaidMember()
        {
            RaidMemberPreset raidMemberPreset = new RaidMemberPreset();
            _preset.RaidMemberPresets.Add(raidMemberPreset);

            RaidMemberViewModel raidMemberViewModel = new RaidMemberViewModel(raidMemberPreset)
            {
                Index = RaidMemberViewModels.Count,
                IsAlliance = IsAlliance
            };

            AddRaidMember(raidMemberViewModel);

            OnPropertyChanged(nameof(RaidMemberViewModels));
            OnPropertyChanged(nameof(CanAddMember));
            OnPropertyChanged(nameof(CanRemoveMember));
        }

        private void AddRaidMember(RaidMemberViewModel raidMemberViewModel)
        {
            RaidMemberViewModels.Add(raidMemberViewModel);
            _activityContainer.AddRaidMember(raidMemberViewModel);
        }
        public void RemoveRaidMember()
        {
            RaidMemberViewModel raidMemberViewModel = RaidMemberViewModels.First(x => x.IsFocused);
            int focusedIndex = RaidMemberViewModels.IndexOf(raidMemberViewModel);

            RaidMemberViewModels.RemoveAt(focusedIndex);
            _preset.RaidMemberPresets.RemoveAt(focusedIndex);
            _activityContainer.RemoveRaidMember(raidMemberViewModel);

            int newIndex = focusedIndex - 1;
            newIndex = Math.Max(newIndex, 0);
            newIndex = Math.Min(newIndex, RaidMemberViewModels.Count - 1);

            RaidMemberViewModels[newIndex].IsFocused = true;

            OnPropertyChanged(nameof(RaidMemberViewModels));
            OnPropertyChanged(nameof(CanAddMember));
            OnPropertyChanged(nameof(CanRemoveMember));
        }
        public void SetMaxRaidSize()
        {
            for (int i = RaidMemberViewModels.Count; i < MaxGroupSize; i++)
                AddRaidMember();
        }
        public void SetCurrentActivty()
        {
            switch (_preset.Activity)
            {
                case ActivityType.WarsongGulch19:
                case ActivityType.WarsongGulch29:
                case ActivityType.WarsongGulch39:
                case ActivityType.WarsongGulch49:
                case ActivityType.WarsongGulch59:
                case ActivityType.WarsongGulch60:
                    _activityContainer = new WarsongGultchActivityManager(_preset.Activity, _preset.RaidLeaderPort, 489);
                    break;
                case ActivityType.ArathiBasin29:
                case ActivityType.ArathiBasin39:
                case ActivityType.ArathiBasin49:
                case ActivityType.ArathiBasin59:
                case ActivityType.ArathiBasin60:
                    _activityContainer = new WarsongGultchActivityManager(_preset.Activity, _preset.RaidLeaderPort, 529);
                    break;
                case ActivityType.AlteracValley:
                    _activityContainer = new WarsongGultchActivityManager(_preset.Activity, _preset.RaidLeaderPort, 30);
                    break;
                case ActivityType.BlackfathomDeeps:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 48);
                    break;
                case ActivityType.BlackrockDepths:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 230);
                    break;
                case ActivityType.BlackwingLair:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 469);
                    break;
                case ActivityType.DireMaul:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 429);
                    break;
                case ActivityType.Gnomeregan:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 90);
                    break;
                case ActivityType.LowerBlackrockSpire:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 229);
                    break;
                case ActivityType.MaraudonEarthSongFalls:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 349);
                    break;
                case ActivityType.MaraudonFoulsporeCavern:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 349);
                    break;
                case ActivityType.MaraudonWickedGrotto:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 349);
                    break;
                case ActivityType.MoltenCore:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 409);
                    break;
                case ActivityType.Naxxramas:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 533);
                    break;
                case ActivityType.OnyxiasLair:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 249);
                    break;
                case ActivityType.RagefireChasm:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 389);
                    break;
                case ActivityType.RazorfenDowns:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 129);
                    break;
                case ActivityType.RazorfenKraul:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 47);
                    break;
                case ActivityType.RuinsOfAhnQiraj:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 509);
                    break;
                case ActivityType.Scholomance:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 289);
                    break;
                case ActivityType.ShadowfangKeep:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 33);
                    break;
                case ActivityType.SMArmory:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 189);
                    break;
                case ActivityType.SMCathedral:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 189);
                    break;
                case ActivityType.SMGraveyard:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 189);
                    break;
                case ActivityType.SMLibrary:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 189);
                    break;
                case ActivityType.StratholmeAlive:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 329);
                    break;
                case ActivityType.StratholmeUndead:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 329);
                    break;
                case ActivityType.TempleOfAhnQiraj:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 531);
                    break;
                case ActivityType.TempleOfAtalHakkar:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 109);
                    break;
                case ActivityType.TheDeadmines:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 36);
                    break;
                case ActivityType.TheStockade:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 34);
                    break;
                case ActivityType.Uldaman:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 70);
                    break;
                case ActivityType.UpperBlackrockSpire:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 229);
                    break;
                case ActivityType.WailingCaverns:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 43);
                    break;
                case ActivityType.ZulFarrak:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 209);
                    break;
                case ActivityType.ZulGurub:
                    _activityContainer = new DungeonActivityManager(_preset.Activity, _preset.RaidLeaderPort, 309);
                    break;
            }

        }
        public ActivityType CurrentActivity
        {
            get
            {
                return _preset.Activity;
            }
            set
            {
                _preset.Activity = value;

                SetCurrentActivty();
                OnPropertyChanged(nameof(CurrentActivity));
            }
        }
        public bool IsAlliance
        {
            get
            {
                return _preset.IsAlliance;
            }
            set
            {
                if (!_preset.IsAlliance)
                {
                    for (int i = 0; i < RaidMemberViewModels.Count; i++)
                    {
                        RaidMemberViewModels[i].SwapFaction();
                    }
                    OnPropertyChanged(nameof(RaidMemberViewModels));
                }
                _preset.IsAlliance = true;

                OnPropertyChanged(nameof(IsAlliance));
                OnPropertyChanged(nameof(IsHorde));
            }
        }
        public bool IsHorde
        {
            get
            {
                return !_preset.IsAlliance;
            }
            set
            {
                if (_preset.IsAlliance)
                {
                    for (int i = 0; i < RaidMemberViewModels.Count; i++)
                    {
                        RaidMemberViewModels[i].SwapFaction();
                    }
                    OnPropertyChanged(nameof(RaidMemberViewModels));
                }
                _preset.IsAlliance = false;

                OnPropertyChanged(nameof(IsAlliance));
                OnPropertyChanged(nameof(IsHorde));
            }
        }

        private bool _shouldRun;
        public bool ShouldRun
        {
            get => _shouldRun;
            set
            {
                _shouldRun = value;

                OnPropertyChanged(nameof(ShouldRun));
                OnPropertyChanged(nameof(CanAddMember));
                OnPropertyChanged(nameof(CanRemoveMember));
            }
        }
        private bool _isFocused;
        public bool IsFocused
        {
            get
            {
                return _isFocused;
            }
            set
            {
                _isFocused = value;

                OnPropertyChanged(nameof(IsFocused));
                OnPropertyChanged(nameof(CanAddMember));
                OnPropertyChanged(nameof(CanRemoveMember));
            }
        }
        public int RaidLeaderPortNumber
        {
            get
            {
                return _preset.RaidLeaderPort;
            }
            set
            {
                _preset.RaidLeaderPort = value;

                OnPropertyChanged(nameof(RaidLeaderPortNumber));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void QueueCommandToProcess(int processId, InstanceCommand command)
        {
            _activityContainer.QueueCommandToProcess(processId, command);
        }

        public void Dispose()
        {
            _processTracker?.Dispose();
        }
    }
}
