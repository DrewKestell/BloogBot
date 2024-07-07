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
        private WorldStateUpdate DefaultActivityCommand { get; } = new WorldStateUpdate() { ActivityAction = ActivityAction.None, ProcessId = Environment.ProcessId };
        private WorldStateUpdate UserActivityCommand { get; set; } = new WorldStateUpdate() { ActivityAction = ActivityAction.None, ProcessId = Environment.ProcessId };
        public WoWStateManagerViewModel() { }

        private ICommand _connectToWoWStateManagerCommand;
        private ICommand _applyWorldStateCommand;
        private ICommand _addActivityCommand;
        private ICommand _removeActivityCommand;
        private ICommand _addActivityMemberCommand;
        private ICommand _removeActivityMemberCommand;
        private ICommand _setMaxRaidSizeCommand;
        private ICommand _setMinRaidSizeCommand;
        public ICommand ConnectToCommand => _connectToWoWStateManagerCommand ??= new CommandHandler(ConnectTo, true);
        public ICommand ApplyWorldStateCommand => _applyWorldStateCommand ??= new CommandHandler(ApplyWorldState, true);
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
        private void ApplyWorldState()
        {
            UserActivityCommand.ActivityAction = ActivityAction.ApplyDesiredState;
            UserActivityCommand.CommandParam1 = SelectedActivityIndex.ToString();
            UserActivityCommand.CommandParam2 = SelectedActivityMemberIndex.ToString();
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
            if (("BehaviorProfile" == propertyName && propertyValue != SelectedActivityMember.BehaviorProfile)
                || ("Account" == propertyName && propertyValue != SelectedActivityMember.Account)
                || ("ProgressionConfig" == propertyName && propertyValue != SelectedActivityMember.ProgressionConfig)
                || ("InitialStateConfig" == propertyName && propertyValue != SelectedActivityMember.InitialStateConfig)
                || ("EndStateConfig" == propertyName && propertyValue != SelectedActivityMember.EndStateConfig))
            {
                UserActivityCommand.ActivityAction = ActivityAction.EditActivityMember;
                UserActivityCommand.CommandParam1 = SelectedActivityIndex.ToString();
                UserActivityCommand.CommandParam2 = SelectedActivityMemberIndex.ToString();
                UserActivityCommand.CommandParam3 = propertyName;
                UserActivityCommand.CommandParam4 = propertyValue;
            }
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

        public bool CanAddActivityMember => SelectedActivity != null && SelectedActivityMemberViewModels.Count < SelectedActivity.ActivityState.MaxActivitySize;
        public bool CanRemoveActivityMember => SelectedActivity != null && SelectedActivityMemberViewModels.Count > SelectedActivity.ActivityState.MinActivitySize;
        private async Task WoWStateHeartbeatTask()
        {
            LogMessage($"{DateTime.Now}| Connecting to WoW State Manager at 127.0.0.1:8088");

            WorldStateCommandClient = new WorldStateCommandClient(8088, IPAddress.Loopback);
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

                    for (int i = 0; i < activityStates.Count; i++)
                        ServerActivityStates.Add(activityStates[i]);

                    try
                    {
                        for (int i = 0; i < ServerActivityStates.Count; i++)
                        {
                            if (i < ActivityViewModels.Count)
                            {
                                ActivityViewModel activityViewModel = ActivityViewModels[i];
                                activityViewModel.ActivityState.ActivityType = ServerActivityStates[i].ActivityType;

                                while (activityViewModel.ActivityState.ActivityMemberStates.Count < ServerActivityStates[i].ActivityMemberStates.Count)
                                {
                                    LogMessage($"Adding new activity member Activity: {i}");
                                    activityViewModel.AddNewActivityMember();
                                }

                                while (activityViewModel.ActivityState.ActivityMemberStates.Count > ServerActivityStates[i].ActivityMemberStates.Count)
                                {
                                    LogMessage($"Removing activity member Activity: {SelectedActivityMember.BehaviorProfile}");
                                    activityViewModel.RemoveActivityMember();
                                }

                                for (int ii = 0; ii < activityViewModel.ActivityState.ActivityMemberStates.Count; ii++)
                                {
                                    activityViewModel.ActivityState.ActivityMemberStates[ii].Account = ServerActivityStates[i].ActivityMemberStates[ii].Account;
                                    activityViewModel.ActivityState.ActivityMemberStates[ii].BehaviorProfile = ServerActivityStates[i].ActivityMemberStates[ii].BehaviorProfile;
                                    activityViewModel.ActivityState.ActivityMemberStates[ii].InitialStateConfig = ServerActivityStates[i].ActivityMemberStates[ii].InitialStateConfig;
                                    activityViewModel.ActivityState.ActivityMemberStates[ii].EndStateConfig = ServerActivityStates[i].ActivityMemberStates[ii].EndStateConfig;
                                }
                            }
                            else
                                ActivityViewModels.Add(new ActivityViewModel(ServerActivityStates[i]));
                        }

                        while (ActivityViewModels.Count > ServerActivityStates.Count)
                        {
                            ActivityViewModels.RemoveAt(ActivityViewModels.Count - 1);
                        }

                        if (ServerActivityStates.Count > 0 && SelectedActivityIndex < 0)
                        {
                            SelectedActivityIndex = 0;
                            SelectedActivity.IsFocused = true;

                            if (SelectedActivityMemberIndex < 0)
                            {
                                SelectedActivityMemberIndex = 0;
                                SelectedActivityMember.IsFocused = true;
                            }
                        }

                        OnPropertyChanged(nameof(ActivityViewModels));
                        OnPropertyChanged(nameof(SelectedActivityIndex));
                        OnPropertyChanged(nameof(SelectedActivity));
                        OnPropertyChanged(nameof(SelectedActivityMemberViewModels));
                        OnPropertyChanged(nameof(SelectedActivityMemberIndex));
                        OnPropertyChanged(nameof(SelectedActivityMember));
                        OnPropertyChanged(nameof(CanAddActivityMember));
                        OnPropertyChanged(nameof(CanRemoveActivityMember));
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"{ex}");
                    }
                });

                await Task.Delay(100);
            }
        }
        private void LogMessage(string message)
        {
            ConsoleLogDataSB.AppendLine(message);

            OnPropertyChanged(nameof(ConsoleLogText));

        }
        private string _wowStateManagerIP = "127.0.0.1";
        private int _wowStateManagerPort = 8088;
        public string WoWStateManagerIP { get { return _wowStateManagerIP; } set { _wowStateManagerIP = value; } }
        public int WoWStateManagerPort { get { return _wowStateManagerPort; } set { _wowStateManagerPort = value; } }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
