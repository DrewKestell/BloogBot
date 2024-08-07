using ActivityManager;
using Communication;
using StateManager.Listeners;
using StateManager.Settings;
using System.Net;

namespace StateManager
{
    public class StateManagerServiceWorker : BackgroundService
    {
        private readonly ILogger<StateManagerServiceWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<uint, IHostedService> _managedServices = [];
        private readonly ActivityManagerSocketListener _activityManagerSocketListener;
        private readonly StateManagerSocketListener _worldStateManagerSocketListener;

        public List<Activity> CurrentActivityList { get; private set; }

        public StateManagerServiceWorker(ILogger<StateManagerServiceWorker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _activityManagerSocketListener = new(configuration["ActivityManagerListenerAddress:IpAddress"], int.Parse(configuration["ActivityManagerListenerPort:Port"]));
            _worldStateManagerSocketListener = new(configuration["StateManagerService:IpAddress"], int.Parse(configuration["StateManagerService:Port"]));

            _activityManagerSocketListener.DataMessageSubject.Subscribe(OnActivityManagerUpdate);
            _worldStateManagerSocketListener.DataMessageSubject.Subscribe(OnWorldStateUpdate);
        }

        public void StartManagedService(uint port)
        {
            var service = ActivatorUtilities.CreateInstance<ActivityManagerServiceWorker>(_serviceProvider);
            _managedServices.Add(port, service);
            ((IHostedService)service).StartAsync(CancellationToken.None).GetAwaiter().GetResult();
            _logger.LogInformation($"Started ActivityManagerService on port: {service.Port}");
        }

        public void StopManagedService(uint port)
        {
            if (_managedServices.TryGetValue(port, out IHostedService? value))
            {
                value.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
                _managedServices.Remove(port);
                _logger.LogInformation($"Stopped ActivityManagerService on port: {port}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StateManagerServiceWorker is running.");

            stoppingToken.Register(() =>
                _logger.LogInformation("StateManagerServiceWorker is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                // Here you can add logic to start/stop services based on certain conditions.
                await Task.Delay(10000, stoppingToken);
            }

            foreach (var service in _managedServices)
            {
                await service.Value.StopAsync(stoppingToken);
            }

            _logger.LogInformation("StateManagerServiceWorker has stopped.");
        }

        private void OnActivityManagerUpdate(DataMessage dataMessage)
        {

        }

        private void OnWorldStateUpdate(DataMessage dataMessage)
        {
            WorldStateUpdate worldStateUpdate = dataMessage.WorldStateUpdate;
            if (worldStateUpdate.Action != ActivityAction.None)
            {
                Console.WriteLine($"{DateTime.Now}|[WorldStateManagerServer]Processing {worldStateUpdate.Action} {worldStateUpdate.Param1} {worldStateUpdate.Param2} {worldStateUpdate.Param3} {worldStateUpdate.Param4}");

                int activityIndex = int.Parse(worldStateUpdate.Param1);
                int activityMemberIndex = int.Parse(worldStateUpdate.Param2);
                switch (worldStateUpdate.Action)
                {
                    case ActivityAction.AddActivity:
                        StateManagerSettings.Instance.ActivityPresets.Add(new Activity());
                        break;
                    case ActivityAction.EditActivity:
                        if (worldStateUpdate.Param3 == "Remove")
                            StateManagerSettings.Instance.ActivityPresets.RemoveAt(activityIndex);
                        else
                            StateManagerSettings.Instance.ActivityPresets[activityIndex].Type = (ActivityType)Enum.Parse(typeof(ActivityType), worldStateUpdate.Param3);

                        break;
                    case ActivityAction.AddActivityMember:
                        StateManagerSettings.Instance.ActivityPresets[activityIndex].Members.Add(new ActivityMember());
                        break;
                    case ActivityAction.EditActivityMember:
                        Activity activityPreset = StateManagerSettings.Instance.ActivityPresets[activityIndex];
                        ActivityMember activityMemberPreset = activityPreset.Members[activityMemberIndex];

                        switch (worldStateUpdate.Param3)
                        {
                            case "BehaviorProfile":
                                activityMemberPreset.BehaviorProfile = worldStateUpdate.Param4;
                                break;
                            case "Account":
                                activityMemberPreset.AccountName = worldStateUpdate.Param4;
                                break;
                            case "ProgressionConfig":
                                activityMemberPreset.ProgressionProfile = worldStateUpdate.Param4;
                                break;
                            case "InitialStateConfig":
                                activityMemberPreset.InitialProfile = worldStateUpdate.Param4;
                                break;
                            case "EndStateConfig":
                                activityMemberPreset.EndStateProfile = worldStateUpdate.Param4;
                                break;
                            case "Remove":
                                activityPreset.Members.RemoveAt(activityMemberIndex);
                                break;
                        }
                        break;
                    case ActivityAction.ApplyDesiredState:
                        StateManagerSettings.Instance.SaveConfig();

                        ApplyDesiredState();
                        break;
                }
            }
            List<Activity> activities = StateManagerSettings.Instance.ActivityPresets.Select(x => new Activity()
            {
                Type = x.Type
            }).ToList();

            activities.ForEach(activity => activity.Members.AddRange(StateManagerSettings.Instance.ActivityPresets[activities.IndexOf(activity)].Members));

            WorldState responseMessage = new();
            responseMessage.Activities.AddRange(activities);

            _worldStateManagerSocketListener.SendMessageToClient(dataMessage.Id, responseMessage);
        }

        private void ApplyDesiredState()
        {
            for (int i = 0; i < StateManagerSettings.Instance.ActivityPresets.Count; i++)
            {
                if (CurrentActivityList.Count < StateManagerSettings.Instance.ActivityPresets.Count)
                {
                    Activity activity = new() { Type = StateManagerSettings.Instance.ActivityPresets[i].Type };
                    activity.Members.AddRange(StateManagerSettings.Instance.ActivityPresets[i].Members);

                    CurrentActivityList.Add(activity);
                    IPAddress listenAddress = IPAddress.Parse("127.0.0.1");
                    IPAddress stateManagerAddress = IPAddress.Parse("127.0.0.1");
                    int stateManagerPort = 5001; // Example port number

                    var scope = _serviceProvider.CreateScope();
                    _managedServices.Add(activity.Port,
                        ActivatorUtilities.CreateInstance<ActivityManagerServiceWorker>(
                            scope.ServiceProvider,
                            listenAddress,
                            activity.Port,
                            stateManagerAddress,
                            stateManagerPort
                        ));

                    _managedServices[activity.Port].StartAsync(CancellationToken.None).GetAwaiter().GetResult();
                }
                else
                {
                    CurrentActivityList[i].Type = StateManagerSettings.Instance.ActivityPresets[i].Type;
                    for (int ii = 0; ii < CurrentActivityList[i].Members.Count; ii++)
                    {
                        if (ii < StateManagerSettings.Instance.ActivityPresets[i].Members.Count)
                        {
                            CurrentActivityList[i].Members[ii] = StateManagerSettings.Instance.ActivityPresets[i].Members[ii];
                        }
                        else
                        {
                            CurrentActivityList[i].Members.Add(StateManagerSettings.Instance.ActivityPresets[i].Members[ii]);
                        }
                    }

                    while (CurrentActivityList[i].Members.Count > StateManagerSettings.Instance.ActivityPresets[i].Members.Count)
                    {
                        CurrentActivityList[i].Members.RemoveAt(CurrentActivityList[i].Members.Count - 1);
                    }
                }
            }
        }
    }
}

