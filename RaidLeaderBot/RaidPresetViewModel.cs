using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RaidLeaderBot
{
    public sealed class RaidPresetViewModel : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public IEnumerable<ActivityType> EnumActivityTypes
        {
            get
            {
                return Enum.GetValues(typeof(ActivityType)).Cast<ActivityType>();
            }
        }
        private readonly RaidPreset _preset;
        public ObservableCollection<RaidMemberViewModel> RaidMemberViewModels { get; set; } = new ObservableCollection<RaidMemberViewModel>();
        public RaidPresetViewModel()
        {
            _preset = new RaidPreset();
        }
        public RaidPresetViewModel(RaidPreset raidPreset)
        {
            _preset = raidPreset;

            if (_preset.RaidMemberPresets == null)
            {
                _preset.RaidMemberPresets = new List<RaidMemberPreset>() { new RaidMemberPreset() };
            }

            RaidMemberViewModels.Clear();

            for (int i = 0; i < _preset.RaidMemberPresets.Count; i++)
            {
                RaidMemberViewModels.Add(new RaidMemberViewModel(_preset.RaidMemberPresets[i]) { Index = i, IsAlliance = IsAlliance });
            }

            OnPropertyChanged(nameof(CurrentActivity));
            OnPropertyChanged(nameof(IsAlliance));
            OnPropertyChanged(nameof(IsHorde));
        }
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
            Console.WriteLine($"StartAllRaidMembers");
        }
        public void StopAllRaidMembers()
        {
            Console.WriteLine($"StopAllRaidMembers");
        }
        public void AddRaidMember()
        {
            _preset.RaidMemberPresets.Add(new RaidMemberPreset());

            RaidMemberViewModels.Add(new RaidMemberViewModel(_preset.RaidMemberPresets.Last()) { Index = RaidMemberViewModels.Count, IsAlliance = IsAlliance });

            OnPropertyChanged(nameof(RaidMemberViewModels));
            OnPropertyChanged(nameof(CanAddMember));
            OnPropertyChanged(nameof(CanRemoveMember));
        }
        public void RemoveRaidMember()
        {
            int focusedIndex = RaidMemberViewModels.IndexOf(RaidMemberViewModels.First(x => x.IsFocused));

            RaidMemberViewModels.RemoveAt(focusedIndex);
            _preset.RaidMemberPresets.RemoveAt(focusedIndex);

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
            for(int i = RaidMemberViewModels.Count; i < MaxGroupSize; i++)
            {
                AddRaidMember();
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
                    for(int i = 0; i < RaidMemberViewModels.Count; i++) {
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
            }
        }
        public bool CanStart
        {
            get
            {
                return !RaidMemberViewModels.All(x => x.ShouldRun);
            }
        }
        public bool CanStop
        {
            get
            {
                return RaidMemberViewModels.Any(x => x.ShouldRun);
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
        private void StartAll()
        {
            try
            {
                for (int i = 0; i < RaidMemberViewModels.Count; i++)
                {
                    RaidMemberViewModels[i].ShouldRun = true;
                }

                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
            }
            catch
            {
                Console.WriteLine("Error encountered starting bot.");
            }
        }
        private void StopAll()
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
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
