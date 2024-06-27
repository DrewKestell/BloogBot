using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WoWActivityMember.Client;
using WoWActivityMember.Models;

namespace WoWStateManagerUI.Views
{
    public sealed class WoWStateManagerViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ActivityState> ServerActivityStates { get; set; } = [];
        public ObservableCollection<ActivityViewModel> ActivityViewModels { get; set; } = [];
        public ObservableCollection<ActivityMemberViewModel> SelectedActivityMemberViewModels => SelectedActivity != null ? SelectedActivity.ActivityMemberViewModels : [];
        public ActivityViewModel? SelectedActivity => ActivityViewModels.Count > 0 && SelectedActivityIndex > -1 ? ActivityViewModels[SelectedActivityIndex] : null;
        public ActivityMemberViewModel? SelectedActivityMember => SelectedActivityMemberViewModels.Count > 0 && SelectedActivityMemberIndex > -1 ? SelectedActivityMemberViewModels[SelectedActivityMemberIndex] : null;
        public StringBuilder ConsoleLogDataSB { get; set; } = new StringBuilder();
        public string ConsoleLogText { get { return ConsoleLogDataSB.ToString(); } set { } }
        private Task HeartbeatTask { get; set; }
        private WorldStateCommandClient WorldStateCommandClient { get; set; }
        private ActivityCommand DefaultActivityCommand { get; } = new ActivityCommand() { ActivityAction = ActivityAction.None, ProcessId = Environment.ProcessId };
        private ActivityCommand UserActivityCommand { get; set; } = new ActivityCommand() { ActivityAction = ActivityAction.None, ProcessId = Environment.ProcessId };
        public WoWStateManagerViewModel() { }

        private ICommand _connectToWoWStateManagerCommand;
        private ICommand _addActivityCommand;
        private ICommand _removeActivityCommand;
        private ICommand _addActivityMemberCommand;
        private ICommand _removeActivityMemberCommand;
        private ICommand _setMaxRaidSizeCommand;
        private ICommand _setMinRaidSizeCommand;
        public ICommand ConnectToCommand => _connectToWoWStateManagerCommand ??= new CommandHandler(ConnectTo, true);
        public ICommand AddActivityCommand => _addActivityCommand ??= new CommandHandler(AddActivity, true);
        public ICommand RemoveActivityCommand => _removeActivityCommand ??= new CommandHandler(RemoveActivity, true);
        public ICommand AddActivityMemberCommand => _addActivityMemberCommand ??= new CommandHandler(AddActivityMember, true);
        public ICommand RemoveActivityMemberCommand => _removeActivityMemberCommand ??= new CommandHandler(RemoveActivityMember, true);
        public ICommand SetMaxRaidSizeCommand => _setMaxRaidSizeCommand ??= new CommandHandler(SetMaxRaidSize, true);
        public ICommand SetMinRaidSizeCommand => _setMinRaidSizeCommand ??= new CommandHandler(SetMinRaidSize, true);

        private void ConnectTo()
        {
            HeartbeatTask?.Dispose();
            HeartbeatTask = Task.Factory.StartNew(WoWStateHeartbeatTask);
        }

        private void AddActivity()
        {
            UserActivityCommand.ActivityAction = ActivityAction.AddActivity;
            UserActivityCommand.CommandParam1 = SelectedActivityIndex.ToString();
            UserActivityCommand.CommandParam2 = SelectedActivityMemberIndex.ToString();
        }

        private void RemoveActivity()
        {
            UserActivityCommand.ActivityAction = ActivityAction.EditActivity;
            UserActivityCommand.CommandParam1 = SelectedActivityIndex.ToString();
            UserActivityCommand.CommandParam2 = SelectedActivityMemberIndex.ToString();
            UserActivityCommand.CommandParam3 = "Remove";
        }

        public void EditActivity()
        {
            UserActivityCommand.ActivityAction = ActivityAction.EditActivity;
            UserActivityCommand.CommandParam1 = SelectedActivityIndex.ToString();
            UserActivityCommand.CommandParam2 = SelectedActivityMemberIndex.ToString();
            UserActivityCommand.CommandParam3 = SelectedActivity.ActivityState.ActivityType.ToString();
        }

        private void AddActivityMember()
        {
            UserActivityCommand.ActivityAction = ActivityAction.AddActivityMember;
            UserActivityCommand.CommandParam1 = SelectedActivityIndex.ToString();
            UserActivityCommand.CommandParam2 = SelectedActivityMemberIndex.ToString();
        }

        private void RemoveActivityMember()
        {
            UserActivityCommand.ActivityAction = ActivityAction.EditActivityMember;
            UserActivityCommand.CommandParam1 = SelectedActivityIndex.ToString();
            UserActivityCommand.CommandParam2 = SelectedActivityMemberIndex.ToString();
            UserActivityCommand.CommandParam3 = "Remove";
        }

        public void EditActivityMember(string propertyName, string propertyValue)
        {
            UserActivityCommand.ActivityAction = ActivityAction.EditActivityMember;
            UserActivityCommand.CommandParam1 = SelectedActivityIndex.ToString();
            UserActivityCommand.CommandParam2 = SelectedActivityMemberIndex.ToString();
            UserActivityCommand.CommandParam3 = propertyName;
            UserActivityCommand.CommandParam4 = propertyValue;
        }

        private void SetMaxRaidSize()
        {
            UserActivityCommand.ActivityAction = ActivityAction.SetMaxMemberSize;
            UserActivityCommand.CommandParam1 = SelectedActivityIndex.ToString();
            UserActivityCommand.CommandParam2 = SelectedActivityMemberIndex.ToString();
        }

        private void SetMinRaidSize()
        {
            UserActivityCommand.ActivityAction = ActivityAction.SetMinMemberSize;
            UserActivityCommand.CommandParam1 = SelectedActivityIndex.ToString();
            UserActivityCommand.CommandParam2 = SelectedActivityMemberIndex.ToString();
        }

        private int _selectedActivityIndex = -1;
        private int _selectedActivityMemberIndex = -1;
        public int SelectedActivityIndex
        {
            get => _selectedActivityIndex;
            set
            {
                if (value > -1 && value < ActivityViewModels.Count)
                {
                    bool resetSelectedMemberIndex = false;
                    if (_selectedActivityIndex != value)
                        resetSelectedMemberIndex = true;

                    _selectedActivityIndex = value;

                    for (int i = 0; i < ActivityViewModels.Count; i++)
                        ActivityViewModels[i].IsFocused = i == _selectedActivityIndex;

                    OnPropertyChanged(nameof(SelectedActivityIndex));
                    OnPropertyChanged(nameof(SelectedActivity));
                    OnPropertyChanged(nameof(CanAddActivityMember));
                    OnPropertyChanged(nameof(CanRemoveActivityMember));

                    if (resetSelectedMemberIndex)
                        SelectedActivityMemberIndex = 0;
                }
            }
        }
        public int SelectedActivityMemberIndex
        {
            get => _selectedActivityMemberIndex;
            set
            {
                if (value > -1 && value < SelectedActivityMemberViewModels.Count)
                {
                    _selectedActivityMemberIndex = value;

                    for (int i = 0; i < SelectedActivityMemberViewModels.Count; i++)
                    {
                        SelectedActivityMemberViewModels[i].IsFocused = i == _selectedActivityMemberIndex;
                    }

                    OnPropertyChanged(nameof(SelectedActivityMemberIndex));
                    OnPropertyChanged(nameof(SelectedActivityMember));
                }
            }
        }

        public ActivityType ActivityType { 
            get {
                return SelectedActivity.CurrentActivity;
            } 
            set 
            { 

            } 
        }

        public bool CanAddActivityMember => SelectedActivity != null && SelectedActivityMemberViewModels.Count < SelectedActivity.ActivityState.MaxActivitySize;
        public bool CanRemoveActivityMember => SelectedActivity != null && SelectedActivityMemberViewModels.Count > SelectedActivity.ActivityState.MinActivitySize;
        private async Task WoWStateHeartbeatTask()
        {
            WorldStateCommandClient = new WorldStateCommandClient(8089, IPAddress.Parse("127.0.0.1"));

            LogMessage($"{DateTime.Now}| Connecting to WoW State Manager at 127.0.0.1:8089");

            while (true)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    List<ActivityState> activityStates = [];
                    if (UserActivityCommand.ActivityAction != ActivityAction.None)
                    {
                        activityStates = WorldStateCommandClient.SendActivityRequest(UserActivityCommand);

                        UserActivityCommand.ActivityAction = ActivityAction.None;
                        UserActivityCommand.CommandParam1 = string.Empty;
                        UserActivityCommand.CommandParam2 = string.Empty;
                        UserActivityCommand.CommandParam3 = string.Empty;
                        UserActivityCommand.CommandParam4 = string.Empty;
                    }
                    else
                        activityStates = WorldStateCommandClient.SendActivityRequest(DefaultActivityCommand);

                    ServerActivityStates.Clear();

                    foreach (var item in activityStates)
                        ServerActivityStates.Add(item);

                    try
                    {
                        foreach (var item in ServerActivityStates)
                        {
                            if (ActivityViewModels.Any(x => x.ActivityState.ProcessId == item.ProcessId))
                            {
                                ActivityViewModel activityViewModel = ActivityViewModels.First(x => x.ActivityState.ProcessId == item.ProcessId);
                                activityViewModel.ActivityState.ActivityType = item.ActivityType;

                                while (activityViewModel.ActivityState.ActivityMemberPresets.Count < item.ActivityMemberPresets.Count)
                                {
                                    activityViewModel.AddNewActivityMember();
                                }

                                while (activityViewModel.ActivityState.ActivityMemberPresets.Count > item.ActivityMemberPresets.Count)
                                {
                                    activityViewModel.RemoveActivityMember();
                                }

                                for (int i = 0; i < activityViewModel.ActivityState.ActivityMemberPresets.Count; i++)
                                {
                                    activityViewModel.ActivityState.ActivityMemberPresets[i].Account = item.ActivityMemberPresets[i].Account;
                                    activityViewModel.ActivityState.ActivityMemberPresets[i].BehaviorProfile = item.ActivityMemberPresets[i].BehaviorProfile;
                                    activityViewModel.ActivityState.ActivityMemberPresets[i].InitialStateConfig = item.ActivityMemberPresets[i].InitialStateConfig;
                                    activityViewModel.ActivityState.ActivityMemberPresets[i].EndStateConfig = item.ActivityMemberPresets[i].EndStateConfig;
                                }
                            }
                            else
                                ActivityViewModels.Add(new ActivityViewModel(item));
                        }

                        if (ServerActivityStates.Count > 0 && SelectedActivityIndex < 0)
                        {
                            SelectedActivityIndex = 0;
                            SelectedActivity.IsFocused = true;
                            OnPropertyChanged(nameof(SelectedActivityIndex));
                            OnPropertyChanged(nameof(SelectedActivity));

                            if (SelectedActivityMemberIndex < 0)
                            {
                                SelectedActivityMemberIndex = 0;
                                SelectedActivityMember.IsFocused = true;
                                OnPropertyChanged(nameof(SelectedActivityMemberIndex));
                                OnPropertyChanged(nameof(SelectedActivityMember));
                            }
                        }

                        OnPropertyChanged(nameof(CanAddActivityMember));
                        OnPropertyChanged(nameof(CanRemoveActivityMember));
                        OnPropertyChanged(nameof(ActivityViewModels));
                        OnPropertyChanged(nameof(SelectedActivityMemberViewModels));
                    }
                    catch (Exception ex)
                    {
                        LogMessage(ex.Message);
                    }
                });

                await Task.Delay(1000);
            }
        }
        private void LogMessage(string message)
        {
            ConsoleLogDataSB.AppendLine(message);

            OnPropertyChanged(nameof(ConsoleLogText));

        }
        private string _wowStateManagerIP = "127.0.0.1";
        private int _wowStateManagerPort = 8089;
        public string WoWStateManagerIP { get { return _wowStateManagerIP; } set { _wowStateManagerIP = value; } }
        public int WoWStateManagerPort { get { return _wowStateManagerPort; } set { _wowStateManagerPort = value; } }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
