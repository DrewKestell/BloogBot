using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WoWActivityMember.Models;

namespace WoWStateManagerUI.Views
{
    public sealed class ActivityViewModel : INotifyPropertyChanged, IDisposable
    {
        public static IEnumerable<ActivityType> EnumActivityTypes => Enum.GetValues(typeof(ActivityType)).Cast<ActivityType>();
        public ActivityState ActivityState { get; private set; } = new();
        public ObservableCollection<ActivityMemberViewModel> ActivityMemberViewModels { get; set; } = [];
        public ActivityViewModel() { }
        public ActivityViewModel(ActivityState activityState)
        {
            ActivityState = activityState;
            ActivityState.ActivityMemberPresets ??= [];

            for (int i = 0; i < ActivityState.ActivityMemberPresets.Count; i++)
            {
                AddActivityMember(ActivityState.ActivityMemberPresets[i]);
            }

            OnPropertyChanged(nameof(ActivityState));
            OnPropertyChanged(nameof(CurrentActivity));
        }

        public void AddNewActivityMember()
        {
            ActivityMemberPreset activityMemberPreset = new();
            ActivityState.ActivityMemberPresets.Add(activityMemberPreset);

            AddActivityMember(activityMemberPreset);

            OnPropertyChanged(nameof(ActivityMemberViewModels));
        }

        public void AddActivityMember(ActivityMemberPreset activityMemberPreset)
        {
            ActivityMemberViewModel activityMemberViewModel = new(activityMemberPreset);
            ActivityMemberViewModels.Add(activityMemberViewModel);
        }
        public void RemoveActivityMember()
        {
            ActivityMemberViewModel activityMemberViewModel = ActivityMemberViewModels.First(x => x.IsFocused);
            int focusedIndex = ActivityMemberViewModels.IndexOf(activityMemberViewModel);

            ActivityMemberViewModels.RemoveAt(focusedIndex);
            ActivityState.ActivityMemberPresets.RemoveAt(focusedIndex);

            int newIndex = focusedIndex - 1;
            newIndex = Math.Max(newIndex, 0);
            newIndex = Math.Min(newIndex, ActivityMemberViewModels.Count - 1);

            ActivityMemberViewModels[newIndex].IsFocused = true;

            OnPropertyChanged(nameof(ActivityMemberViewModels));
        }

        public ActivityType CurrentActivity
        {
            get => ActivityState.ActivityType;
            set
            {
                ActivityState.ActivityType = value;

                OnPropertyChanged(nameof(CurrentActivity));
            }
        }

        private bool _isApplied;
        public bool IsApplied
        {
            get => _isApplied;
            set
            {
                _isApplied = value;

                OnPropertyChanged(nameof(IsApplied));
            }
        }
        private bool _isFocused;
        public bool IsFocused
        {
            get => _isFocused;
            set
            {
                _isFocused = value;

                OnPropertyChanged(nameof(IsFocused));
            }
        }
        //public int MinLevelRequirement
        //{
        //    get
        //    {
        //        switch (CurrentActivity)
        //        {
        //            case ActivityType.PvERagefireChasm:
        //                return 8;
        //            case ActivityType.PvPWarsongGulch19:
        //            case ActivityType.PvEShadowfangKeep:
        //            case ActivityType.PvETheDeadmines:
        //            case ActivityType.PvEWailingCaverns:
        //                return 10;
        //            case ActivityType.PvETheStockade:
        //                return 15;
        //            case ActivityType.PvERazorfenKraul:
        //                return 17;
        //            case ActivityType.PvEBlackfathomDeeps:
        //                return 19;
        //            case ActivityType.PvPArathiBasin29:
        //            case ActivityType.PvPWarsongGulch29:
        //            case ActivityType.PvEGnomeregan:
        //            case ActivityType.PvESMGraveyard:
        //            case ActivityType.PvESMLibrary:
        //            case ActivityType.PvESMArmory:
        //            case ActivityType.PvESMCathedral:
        //                return 20;
        //            case ActivityType.PvERazorfenDowns:
        //                return 25;
        //            case ActivityType.PvPArathiBasin39:
        //            case ActivityType.PvPWarsongGulch39:
        //            case ActivityType.PvEUldaman:
        //            case ActivityType.PvEMaraudonEarthSongFalls:
        //            case ActivityType.PvEMaraudonFoulsporeCavern:
        //            case ActivityType.PvEMaraudonWickedGrotto:
        //                return 30;
        //            case ActivityType.PvETempleOfAtalHakkar:
        //            case ActivityType.PvEZulFarrak:
        //                return 35;
        //            case ActivityType.PvPArathiBasin49:
        //            case ActivityType.PvPWarsongGulch49:
        //            case ActivityType.PvEBlackrockDepths:
        //                return 40;
        //            case ActivityType.PvELowerBlackrockSpire:
        //            case ActivityType.PvEUpperBlackrockSpire:
        //            case ActivityType.PvEScholomance:
        //            case ActivityType.PvEStratholmeAlive:
        //            case ActivityType.PvEStratholmeUndead:
        //            case ActivityType.PvEDireMaul:
        //                return 45;
        //            case ActivityType.PvPArathiBasin59:
        //            case ActivityType.PvPWarsongGulch59:
        //            case ActivityType.PvEMoltenCore:
        //            case ActivityType.PvEOnyxiasLair:
        //            case ActivityType.PvEZulGurub:
        //                return 50;
        //            case ActivityType.PvPAlteracValley:
        //                return 51;
        //            case ActivityType.PvPArathiBasin60:
        //            case ActivityType.PvPWarsongGulch60:
        //            case ActivityType.PvEBlackwingLair:
        //            case ActivityType.PvERuinsOfAhnQiraj:
        //            case ActivityType.PvETempleOfAhnQiraj:
        //            case ActivityType.PvENaxxramas:
        //                return 60;
        //        }
        //        return 1;
        //    }
        //}
        //public int MaxLevelRequirement
        //{
        //    get
        //    {
        //        switch (CurrentActivity)
        //        {
        //            case ActivityType.PvPWarsongGulch19:
        //                return 19;
        //            case ActivityType.PvPWarsongGulch29:
        //            case ActivityType.PvPArathiBasin29:
        //                return 29;
        //            case ActivityType.PvPWarsongGulch39:
        //            case ActivityType.PvPArathiBasin39:
        //                return 39;
        //            case ActivityType.PvPWarsongGulch49:
        //            case ActivityType.PvPArathiBasin49:
        //                return 49;
        //            case ActivityType.PvPWarsongGulch59:
        //            case ActivityType.PvPArathiBasin59:
        //                return 59;
        //        }
        //        return 60;
        //    }
        //}
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {

        }
    }
}
