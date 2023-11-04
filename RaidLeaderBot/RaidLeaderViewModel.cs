using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RaidLeaderBot
{
    public sealed class RaidLeaderViewModel : INotifyPropertyChanged
    {
        private ActivityContainer _activityContainer;
        public ObservableCollection<int> ActivityPresetIndexes { get; set; } = new ObservableCollection<int>();
        public ObservableCollection<RaidPresetViewModel> RaidPresetViewModels { get; set; } = new ObservableCollection<RaidPresetViewModel>();
        public ObservableCollection<RaidMemberViewModel> RaidMemberViewModels { get; set; } = new ObservableCollection<RaidMemberViewModel>();
        public RaidLeaderViewModel()
        {
            ActivityPresetIndexes.Clear();

            for (int i = 0; i < RaidLeaderBotSettings.Instance.ActivityPresets.Count; i++)
            {
                ActivityPresetIndexes.Add(i + 1);
            }

            ActivityPresetIndex = 1;
            SelectedRaidMemberIndex = 0;

            _activityContainer = new ActivityContainer();

            OnPropertyChanged(nameof(ActivityPresetIndexes));
        }
        public void AddPreset()
        {
            RaidLeaderBotSettings.Instance.ActivityPresets.Add(new List<RaidPreset>());

            RaidLeaderBotSettings.Instance.ActivityPresets
                .Last()
                .Add(new RaidPreset());

            ActivityPresetIndexes.Add(ActivityPresetIndexes.Count + 1);

            ActivityPresetIndex = ActivityPresetIndexes.Count;

            SetRaidFocusState (0, true);

            OnPropertyChanged(nameof(ActivityPresetIndexes));
            OnPropertyChanged(nameof(CanAddPreset));
            OnPropertyChanged(nameof(CanRemovePreset));
        }
        public void RemovePreset()
        {
            ActivityPresetIndexes.RemoveAt(ActivityPresetIndexes.Count - 1);

            RaidLeaderBotSettings.Instance.ActivityPresets.RemoveAt(_activityPresetIndex);

            int newIndexValue = _activityPresetIndex - 1;
            newIndexValue = Math.Max(newIndexValue, 0);
            newIndexValue = Math.Min(newIndexValue, ActivityPresetIndexes.Count - 1);

            ActivityPresetIndex = newIndexValue + 1;

            OnPropertyChanged(nameof(ActivityPresetIndexes));
            OnPropertyChanged(nameof(CanAddPreset));
            OnPropertyChanged(nameof(CanRemovePreset));
        }
        public void AddRaid()
        {
            for (int i = 0; i < RaidPresetViewModels.Count; i++)
            {
                SetRaidFocusState(i, false);
            }

            RaidLeaderBotSettings.Instance.ActivityPresets[_activityPresetIndex]
                .Add(new RaidPreset() { RaidMemberPresets = new List<RaidMemberPreset>() { new RaidMemberPreset() }, Activity = RaidLeaderBotSettings.Instance.ActivityPresets[_activityPresetIndex][SelectedRaidIndex].Activity });

            RaidPresetViewModels.Add(new RaidPresetViewModel(RaidLeaderBotSettings.Instance.ActivityPresets[_activityPresetIndex].Last()) { Index = RaidPresetViewModels.Count });

            SetRaidFocusState(RaidPresetViewModels.Count - 1, true);

            OnPropertyChanged(nameof(CanAddRaid));
            OnPropertyChanged(nameof(CanRemoveRaid));
        }
        public void RemoveRaid()
        {
            RaidLeaderBotSettings.Instance.ActivityPresets[ActivityPresetIndex - 1]
                .RemoveAt(SelectedRaidIndex);

            RaidPresetViewModels.RemoveAt(SelectedRaidIndex);

            int newIndex = SelectedRaidIndex - 1;
            newIndex = Math.Max(newIndex, 0);
            newIndex = Math.Min(newIndex, RaidLeaderBotSettings.Instance.ActivityPresets[ActivityPresetIndex - 1].Count - 1);

            SetRaidFocusState(newIndex, true);

            OnPropertyChanged(nameof(CanAddRaid));
            OnPropertyChanged(nameof(CanRemoveRaid));
        }
        public void SaveConfig()
        {
            RaidLeaderBotSettings.Instance.SaveConfig();
        }
        public void SetRaidFocusState(int index, bool isFocused)
        {
            RaidPresetViewModels[index].IsFocused = isFocused;
            if (isFocused)
            {
                SelectedRaidIndex = index;

                SetMemberFocusState(0, true);
            }
        }
        public void SetMemberFocusState(int index, bool isFocused)
        {
            RaidPresetViewModels[SelectedRaidIndex].RaidMemberViewModels[index].IsFocused = isFocused;
            if (isFocused)
            {
                SelectedRaidMemberIndex = index;
            }
        }
        private int _activityPresetIndex;
        public int ActivityPresetIndex
        {
            get { return _activityPresetIndex + 1; }
            set
            {
                _activityPresetIndex = value - 1;

                RegenerateRaidPresetCollection();
                SetRaidFocusState(0, true);

                OnPropertyChanged(nameof(ActivityPresetIndex));
            }
        }
        private int _selectedRaidIndex;
        public int SelectedRaidIndex
        {
            get { return _selectedRaidIndex; }
            set
            {
                _selectedRaidIndex = value;

                RenegerateRaidMembersCollection();
                SetMemberFocusState(0, true);

                OnPropertyChanged(nameof(SelectedRaidIndex));
            }
        }
        private int _selectedRaidMemberIndex;
        public int SelectedRaidMemberIndex
        {
            get { return _selectedRaidMemberIndex; }
            set
            {
                _selectedRaidMemberIndex = value;

                OnPropertyChanged(nameof(SelectedRaidMemberIndex));
            }
        }
        private void RegenerateRaidPresetCollection()
        {
            for (int i = 0; i < RaidPresetViewModels.Count; i++)
            {
                SetRaidFocusState(i, false);
            }

            RaidPresetViewModels.Clear();

            List<RaidPreset> raidPresets = RaidLeaderBotSettings.Instance.ActivityPresets[ActivityPresetIndex - 1];

            for (int i = 0; i < raidPresets.Count; i++)
            {
                RaidPresetViewModels.Add(new RaidPresetViewModel(raidPresets[i]) { Index = i });
            }

            OnPropertyChanged(nameof(RaidPresetViewModels));
            OnPropertyChanged(nameof(CanAddRaid));
            OnPropertyChanged(nameof(CanRemoveRaid));
        }
        private void RenegerateRaidMembersCollection()
        {
            for (int i = 0; i < RaidPresetViewModels[SelectedRaidIndex].RaidMemberViewModels.Count; i++)
            {
                SetMemberFocusState(i, false);
            }

            RaidMemberViewModels = RaidPresetViewModels[SelectedRaidIndex].RaidMemberViewModels;

            OnPropertyChanged(nameof(RaidMemberViewModels));
        }
        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;

                OnPropertyChanged(nameof(IsRunning));

                OnPropertyChanged(nameof(CanAddRaid));
                OnPropertyChanged(nameof(CanRemoveRaid));
                OnPropertyChanged(nameof(CanAddPreset));
                OnPropertyChanged(nameof(CanRemovePreset));
            }
        }
        public bool CanAddPreset => !IsRunning && RaidLeaderBotSettings.Instance.ActivityPresets.Count < 10;
        public bool CanRemovePreset => !IsRunning && RaidLeaderBotSettings.Instance.ActivityPresets.Count > 1;
        public bool CanAddRaid => !IsRunning && RaidLeaderBotSettings.Instance.ActivityPresets[ActivityPresetIndex - 1].Count < 4;
        public bool CanRemoveRaid => !IsRunning && RaidLeaderBotSettings.Instance.ActivityPresets[ActivityPresetIndex - 1].Count > 1;

        public ICommand SaveConfigCommand => _saveConfigCommand ??= new CommandHandler(SaveConfig, true);
        public ICommand AddPresetCommand => _addPresetCommand ??= new CommandHandler(AddPreset, true);
        public ICommand RemovePresetCommand => _removePresetCommand ??= new CommandHandler(RemovePreset, true);
        public ICommand AddRaidCommand => _addRaidCommand ??= new CommandHandler(AddRaid, true);
        public ICommand RemoveRaidCommand => _removeRaidCommand ??= new CommandHandler(RemoveRaid, true);

        private ICommand _saveConfigCommand;
        private ICommand _addPresetCommand;
        private ICommand _removePresetCommand;

        private ICommand _addRaidCommand;
        private ICommand _removeRaidCommand;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
