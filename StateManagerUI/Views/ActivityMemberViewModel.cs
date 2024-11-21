using Communication;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StateManagerUI.Views
{
    public sealed class ActivityMemberViewModel : INotifyPropertyChanged
    {
        private readonly ActivitySnapshot _activityMemberPreset;

        public ActivityMemberViewModel() => _activityMemberPreset = new ActivitySnapshot();
        public ActivityMemberViewModel(ActivitySnapshot activityMemberPreset)
        {
            _activityMemberPreset = activityMemberPreset;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
