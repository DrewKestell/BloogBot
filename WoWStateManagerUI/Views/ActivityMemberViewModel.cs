using System.ComponentModel;
using System.Runtime.CompilerServices;
using WoWClientBot.Models;

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

            OnPropertyChanged(nameof(AccountName));
            OnPropertyChanged(nameof(BotProfileName));
            OnPropertyChanged(nameof(BeginStateConfigName));
            OnPropertyChanged(nameof(EndStateConfigName));
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

        public string BotProfileName
        {
            get => _activityMemberPreset.BotProfileName;
            set
            {
                _activityMemberPreset.BotProfileName = value;

                OnPropertyChanged(nameof(BotProfileName));
            }
        }
        public string BeginStateConfigName
        {
            get => _activityMemberPreset.BeginStateConfigName;
            set
            {
                _activityMemberPreset.BeginStateConfigName = value;
                OnPropertyChanged(nameof(BeginStateConfigName));
            }
        }
        public string EndStateConfigName
        {
            get => _activityMemberPreset.EndStateConfigName;
            set
            {
                _activityMemberPreset.EndStateConfigName = value;
                OnPropertyChanged(nameof(EndStateConfigName));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
