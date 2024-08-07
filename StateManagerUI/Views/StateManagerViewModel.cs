using Communication;
using StateManager.Clients;
using StateManagerUI.Handlers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace StateManagerUI.Views
{
    public sealed class StateManagerViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Activity> ServerActivityStates { get; set; } = [];
        public ObservableCollection<ActivityViewModel> ActivityViewModels { get; set; } = [];
        public ObservableCollection<ActivityMemberViewModel> SelectedActivityMemberViewModels => SelectedActivity != null ? SelectedActivity.ActivityMemberViewModels : [];
        public ActivityViewModel? SelectedActivity => ActivityViewModels.Count > 0 && SelectedActivityIndex > -1 ? ActivityViewModels[SelectedActivityIndex] : null;
        public ActivityMemberViewModel? SelectedActivityMember => SelectedActivityMemberViewModels.Count > 0 && SelectedActivityMemberIndex > -1 ? SelectedActivityMemberViewModels[SelectedActivityMemberIndex] : null;
        public StringBuilder ConsoleLogDataSB { get; set; } = new StringBuilder();
        public string ConsoleLogText { get { return ConsoleLogDataSB.ToString(); } set { } }
        private Task HeartbeatTask { get; set; }
        private StateManagerUpdateClient StateManagerUpdateClient { get; set; }
        private WorldStateUpdate DefaultWorldStateUpdate { get; } = new WorldStateUpdate() { Action = ActivityAction.None };
        private WorldStateUpdate WorldStateUpdate { get; set; } = new WorldStateUpdate() { Action = ActivityAction.None };
        public StateManagerViewModel() { }

        private ICommand _connectToStateManagerCommand;
        private ICommand _applyWorldStateCommand;
        private ICommand _addActivityCommand;
        private ICommand _removeActivityCommand;
        private ICommand _addActivityMemberCommand;
        private ICommand _removeActivityMemberCommand;
        private ICommand _setMaxRaidSizeCommand;
        private ICommand _setMinRaidSizeCommand;
        public ICommand ConnectToCommand => _connectToStateManagerCommand ??= new CommandHandler(ConnectTo, true);
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
            WorldStateUpdate.Action = ActivityAction.ApplyDesiredState;
            WorldStateUpdate.Param1 = SelectedActivityIndex.ToString();
            WorldStateUpdate.Param2 = SelectedActivityMemberIndex.ToString();
        }

        private void AddActivity()
        {
            WorldStateUpdate.Action = ActivityAction.AddActivity;
            WorldStateUpdate.Param1 = SelectedActivityIndex.ToString();
            WorldStateUpdate.Param2 = SelectedActivityMemberIndex.ToString();
        }

        private void RemoveActivity()
        {
            WorldStateUpdate.Action = ActivityAction.EditActivity;
            WorldStateUpdate.Param1 = SelectedActivityIndex.ToString();
            WorldStateUpdate.Param2 = SelectedActivityMemberIndex.ToString();
            WorldStateUpdate.Param3 = "Remove";
        }

        public void EditActivity()
        {
            WorldStateUpdate.Action = ActivityAction.EditActivity;
            WorldStateUpdate.Param1 = SelectedActivityIndex.ToString();
            WorldStateUpdate.Param2 = SelectedActivityMemberIndex.ToString();
            WorldStateUpdate.Param3 = SelectedActivity.ActivityState.Type.ToString();
        }

        private void AddActivityMember()
        {
            WorldStateUpdate.Action = ActivityAction.AddActivityMember;
            WorldStateUpdate.Param1 = SelectedActivityIndex.ToString();
            WorldStateUpdate.Param2 = SelectedActivityMemberIndex.ToString();
        }

        private void RemoveActivityMember()
        {
            WorldStateUpdate.Action = ActivityAction.EditActivityMember;
            WorldStateUpdate.Param1 = SelectedActivityIndex.ToString();
            WorldStateUpdate.Param2 = SelectedActivityMemberIndex.ToString();
            WorldStateUpdate.Param3 = "Remove";
        }

        public void EditActivityMember(string propertyName, string propertyValue)
        {
            if ("BehaviorProfile" == propertyName && propertyValue != SelectedActivityMember.BehaviorProfile
                || "Account" == propertyName && propertyValue != SelectedActivityMember.Account
                || "ProgressionConfig" == propertyName && propertyValue != SelectedActivityMember.ProgressionConfig
                || "InitialStateConfig" == propertyName && propertyValue != SelectedActivityMember.InitialStateConfig
                || "EndStateConfig" == propertyName && propertyValue != SelectedActivityMember.EndStateConfig)
            {
                WorldStateUpdate.Action = ActivityAction.EditActivityMember;
                WorldStateUpdate.Param1 = SelectedActivityIndex.ToString();
                WorldStateUpdate.Param2 = SelectedActivityMemberIndex.ToString();
                WorldStateUpdate.Param3 = propertyName;
                WorldStateUpdate.Param4 = propertyValue;
            }
        }

        private void SetMaxRaidSize()
        {
            WorldStateUpdate.Action = ActivityAction.SetMaxMemberSize;
            WorldStateUpdate.Param1 = SelectedActivityIndex.ToString();
            WorldStateUpdate.Param2 = SelectedActivityMemberIndex.ToString();
        }

        private void SetMinRaidSize()
        {
            WorldStateUpdate.Action = ActivityAction.SetMinMemberSize;
            WorldStateUpdate.Param1 = SelectedActivityIndex.ToString();
            WorldStateUpdate.Param2 = SelectedActivityMemberIndex.ToString();
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

        public bool CanAddActivityMember => SelectedActivity != null;
        public bool CanRemoveActivityMember => SelectedActivity != null;
        private async Task WoWStateHeartbeatTask()
        {
            LogMessage($"{DateTime.Now}| Connecting to WoW State Manager at 127.0.0.1:8088");

            StateManagerUpdateClient = new StateManagerUpdateClient(IPAddress.Loopback.ToString(), 8088);
            while (true)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    List<Activity> activityStates = [];
                    if (WorldStateUpdate.Action != ActivityAction.None)
                    {
                        activityStates = [.. StateManagerUpdateClient.SendWorldStateUpdate(WorldStateUpdate).Activities];

                        WorldStateUpdate.Action = ActivityAction.None;
                        WorldStateUpdate.Param1 = string.Empty;
                        WorldStateUpdate.Param2 = string.Empty;
                        WorldStateUpdate.Param3 = string.Empty;
                        WorldStateUpdate.Param4 = string.Empty;
                    }
                    else
                        activityStates = [.. StateManagerUpdateClient.SendWorldStateUpdate(DefaultWorldStateUpdate).Activities];

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
                                activityViewModel.ActivityState.Type = ServerActivityStates[i].Type;

                                while (activityViewModel.ActivityState.Members.Count < ServerActivityStates[i].Members.Count)
                                {
                                    LogMessage($"Adding new activity member Activity: {i}");
                                    activityViewModel.AddNewActivityMember();
                                }

                                while (activityViewModel.ActivityState.Members.Count > ServerActivityStates[i].Members.Count)
                                {
                                    LogMessage($"Removing activity member Activity: {SelectedActivityMember.BehaviorProfile}");
                                    activityViewModel.RemoveActivityMember();
                                }

                                for (int ii = 0; ii < activityViewModel.ActivityState.Members.Count; ii++)
                                {
                                    activityViewModel.ActivityState.Members[ii].AccountName = ServerActivityStates[i].Members[ii].AccountName;
                                    activityViewModel.ActivityState.Members[ii].BehaviorProfile = ServerActivityStates[i].Members[ii].BehaviorProfile;
                                    activityViewModel.ActivityState.Members[ii].InitialProfile = ServerActivityStates[i].Members[ii].InitialProfile;
                                    activityViewModel.ActivityState.Members[ii].EndStateProfile = ServerActivityStates[i].Members[ii].EndStateProfile;
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
        public string StateManagerIP { get { return _wowStateManagerIP; } set { _wowStateManagerIP = value; } }
        public int StateManagerPort { get { return _wowStateManagerPort; } set { _wowStateManagerPort = value; } }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
