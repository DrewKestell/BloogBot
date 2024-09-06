using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StateManagerUI.Views
{
    public sealed class ActivityMemberViewModel : INotifyPropertyChanged
    {
        private readonly ActivityMember _activityMemberPreset;

        public ActivityMemberViewModel() => _activityMemberPreset = new ActivityMember();
        public ActivityMemberViewModel(ActivityMember activityMemberPreset)
        {
            _activityMemberPreset = activityMemberPreset;

            OnPropertyChanged(nameof(AccountName));
            OnPropertyChanged(nameof(BehaviorProfile));
            OnPropertyChanged(nameof(ProgressionProfile));
            OnPropertyChanged(nameof(InitialProfile));
            OnPropertyChanged(nameof(EndStateProfile));
            OnPropertyChanged(nameof(IsFocused));
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
        public string AccountName
        {
            get => _activityMemberPreset.AccountName;
            set
            {
                _activityMemberPreset.AccountName = value;
                OnPropertyChanged(nameof(AccountName));
            }
        }
        public string BehaviorProfile
        {
            get => _activityMemberPreset.BehaviorProfile;
            set
            {
                _activityMemberPreset.BehaviorProfile = value;

                OnPropertyChanged(nameof(BehaviorProfile));
            }
        }
        public string ProgressionProfile
        {
            get => _activityMemberPreset.ProgressionProfile;
            set
            {
                _activityMemberPreset.ProgressionProfile = value;

                OnPropertyChanged(nameof(ProgressionProfile));
            }
        }
        public string InitialProfile
        {
            get => _activityMemberPreset.InitialProfile;
            set
            {
                _activityMemberPreset.InitialProfile = value;
                OnPropertyChanged(nameof(InitialProfile));
            }
        }
        public string EndStateProfile
        {
            get => _activityMemberPreset.EndStateProfile;
            set
            {
                _activityMemberPreset.EndStateProfile = value;
                OnPropertyChanged(nameof(EndStateProfile));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
