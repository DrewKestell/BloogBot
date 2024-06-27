using System.ComponentModel;
using System.Runtime.CompilerServices;
using WoWActivityMember.Models;

namespace WoWStateManagerUI.Views
{
    public sealed class ActivityMemberViewModel : INotifyPropertyChanged
    {
        private readonly ActivityMemberPreset _activityMemberPreset;

        public ActivityMemberViewModel()
        {
            _activityMemberPreset = new ActivityMemberPreset();
        }
        public ActivityMemberViewModel(ActivityMemberPreset activityMemberPreset)
        {
            _activityMemberPreset = activityMemberPreset;

            OnPropertyChanged(nameof(Account));
            OnPropertyChanged(nameof(BehaviorProfile));
            OnPropertyChanged(nameof(ProgressionConfig));
            OnPropertyChanged(nameof(InitialStateConfig));
            OnPropertyChanged(nameof(EndStateConfig));
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
        public string Account
        {
            get => _activityMemberPreset.Account;
            set
            {
                _activityMemberPreset.Account = value;
                OnPropertyChanged(nameof(Account));
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
        public string ProgressionConfig
        {
            get => _activityMemberPreset.ProgressionConfig;
            set
            {
                _activityMemberPreset.ProgressionConfig = value;

                OnPropertyChanged(nameof(ProgressionConfig));
            }
        }
        public string InitialStateConfig
        {
            get => _activityMemberPreset.InitialStateConfig;
            set
            {
                _activityMemberPreset.InitialStateConfig = value;
                OnPropertyChanged(nameof(InitialStateConfig));
            }
        }
        public string EndStateConfig
        {
            get => _activityMemberPreset.EndStateConfig;
            set
            {
                _activityMemberPreset.EndStateConfig = value;
                OnPropertyChanged(nameof(EndStateConfig));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
