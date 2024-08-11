using Communication;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StateManagerUI.Views
{
    public sealed class ActivityViewModel : INotifyPropertyChanged, IDisposable
    {
        public static IEnumerable<ActivityType> EnumActivityTypes => Enum.GetValues(typeof(ActivityType)).Cast<ActivityType>();
        public List<ActivityMember> ActivityMembers { get; private set; } = [];
        public ObservableCollection<ActivityMemberViewModel> ActivityMemberViewModels { get; set; } = [];
        public ActivityViewModel() { }
        public ActivityViewModel(List<ActivityMember> activityMembers)
        {
            ActivityMembers = activityMembers;

            for (int i = 0; i < activityMembers.Count; i++)
                AddActivityMember(activityMembers[i]);

            //OnPropertyChanged(nameof(ActivityState));
            OnPropertyChanged(nameof(CurrentActivity));
        }

        public void AddNewActivityMember()
        {
            ActivityMember activityMemberPreset = new();
            //ActivityState.Members.Add(activityMemberPreset);

            AddActivityMember(activityMemberPreset);

            OnPropertyChanged(nameof(ActivityMemberViewModels));
        }

        public void AddActivityMember(ActivityMember activityMemberPreset)
        {
            ActivityMemberViewModel activityMemberViewModel = new(activityMemberPreset);
            ActivityMemberViewModels.Add(activityMemberViewModel);
        }
        public void RemoveActivityMember()
        {
            ActivityMemberViewModel activityMemberViewModel = ActivityMemberViewModels.First(x => x.IsFocused);
            int focusedIndex = ActivityMemberViewModels.IndexOf(activityMemberViewModel);

            ActivityMemberViewModels.RemoveAt(focusedIndex);
            //ActivityState.Members.RemoveAt(focusedIndex);

            int newIndex = focusedIndex - 1;
            newIndex = Math.Max(newIndex, 0);
            newIndex = Math.Min(newIndex, ActivityMemberViewModels.Count - 1);

            ActivityMemberViewModels[newIndex].IsFocused = true;

            OnPropertyChanged(nameof(ActivityMemberViewModels));
        }

        public ActivityType CurrentActivity { get { return ActivityMembers.First().Type; } set { } }

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

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {

        }
    }
}
