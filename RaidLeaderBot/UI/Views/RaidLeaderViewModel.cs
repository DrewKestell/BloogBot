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
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace RaidLeaderBot
{
    public sealed class RaidLeaderViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly List<ActivityManager> _activityContainer;
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
            _activityContainer = new List<ActivityManager>() { new DungeonActivityManager(_preset.RaidLeaderPort, 389) };
            _processTracker = new ProcessTracker();
            _preset.RaidMemberPresets ??= new List<RaidMemberPreset>() { new RaidMemberPreset() };

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

        public CharacterState GetCharacterStateByRaidMemberViewModel(RaidMemberViewModel index) => _activityContainer[0].PartyMembersToStates[index];

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

                    if (_activityContainer[0].PartyMembersToStates[RaidMemberViewModels[i]].ProcessId == 0)
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
                            await Task.Delay(5000);
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
            _activityContainer[0].AddRaidMember(raidMemberViewModel);
        }
        public void RemoveRaidMember()
        {
            RaidMemberViewModel raidMemberViewModel = RaidMemberViewModels.First(x => x.IsFocused);
            int focusedIndex = RaidMemberViewModels.IndexOf(raidMemberViewModel);

            RaidMemberViewModels.RemoveAt(focusedIndex);
            _preset.RaidMemberPresets.RemoveAt(focusedIndex);
            _activityContainer[0].RemoveRaidMember(raidMemberViewModel);

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
        public ActivityType CurrentActivity
        {
            get
            {
                return _preset.Activity;
            }
            set
            {
                _preset.Activity = value;
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
            _activityContainer[0].QueueCommandToProcess(processId, command);
        }

        public void Dispose()
        {
            _processTracker?.Dispose();
        }
    }
}
