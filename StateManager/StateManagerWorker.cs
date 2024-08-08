using ActivityManager;
using Communication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using StateManager.Listeners;
using StateManager.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using BotCommLayer;

namespace StateManager
{
    public class StateManagerWorker : BackgroundService
    {
        private readonly ILogger<StateManagerWorker> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        private readonly Dictionary<uint, IHostedService> _managedServices = [];

        private readonly ActivityManagerSocketListener _activityManagerSocketListener;
        private readonly StateManagerSocketListener _worldStateManagerSocketListener;

        public List<Activity> CurrentActivityList { get; private set; } = [];

        public StateManagerWorker(
            ILogger<StateManagerWorker> logger,
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _serviceProvider = serviceProvider;
            _configuration = configuration;

            _activityManagerSocketListener = new ActivityManagerSocketListener(
                configuration["ActivityManagerListener:IpAddress"],
                int.Parse(configuration["ActivityManagerListener:Port"]),
                _loggerFactory.CreateLogger<ActivityManagerSocketListener>()
            );

            _logger.LogInformation($"Started ActivityManagerListener| {configuration["ActivityManagerListener:IpAddress"]}:{configuration["ActivityManagerListener:Port"]}");

            _worldStateManagerSocketListener = new StateManagerSocketListener(
                configuration["StateManagerListener:IpAddress"],
                int.Parse(configuration["StateManagerListener:Port"]),
                _loggerFactory.CreateLogger<StateManagerSocketListener>()
            );

            _logger.LogInformation($"Started StateManagerListener| {configuration["StateManagerListener:IpAddress"]}:{configuration["StateManagerListener:Port"]}");

            _activityManagerSocketListener.DataMessageSubject.Subscribe(OnActivityManagerUpdate);
            _worldStateManagerSocketListener.DataMessageSubject.Subscribe(OnWorldStateUpdate);
        }

        public void StartManagedService(uint port)
        {
            var scope = _serviceProvider.CreateScope();
            var service = ActivatorUtilities.CreateInstance<ActivityManagerWorker>(
                scope.ServiceProvider,
                _loggerFactory.CreateLogger<ActivityManagerWorker>(),
                Options.Create(new AppSettings() { ListenPort = port })
            );
            _managedServices.Add(port, service);
            ((IHostedService)service).StartAsync(CancellationToken.None).GetAwaiter().GetResult();
            _logger.LogInformation($"Started ActivityManagerService on port: {port}");
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
            _logger.LogInformation($"StateManagerServiceWorker is running.");

            stoppingToken.Register(() =>
                _logger.LogInformation($"StateManagerServiceWorker is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                // Here you can add logic to start/stop services based on certain conditions.
                await Task.Delay(10000, stoppingToken);
            }

            foreach (var service in _managedServices)
            {
                await service.Value.StopAsync(stoppingToken);
            }

            _logger.LogInformation($"StateManagerServiceWorker has stopped.");
        }

        private void OnActivityManagerUpdate(DataMessage dataMessage)
        {
            // Handle ActivityManager updates
        }

        private void OnWorldStateUpdate(DataMessage dataMessage)
        {
            WorldStateUpdate worldStateUpdate = dataMessage.WorldStateUpdate;
            if (worldStateUpdate.Action != ActivityAction.None)
            {
                _logger.LogInformation($"[OnWorldStateUpdate]Processing {worldStateUpdate.Action} {worldStateUpdate.Param1} {worldStateUpdate.Param2} {worldStateUpdate.Param3} {worldStateUpdate.Param4}");

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
                            case "AccountName":
                                activityMemberPreset.AccountName = worldStateUpdate.Param4;
                                break;
                            case "ProgressionProfile":
                                activityMemberPreset.ProgressionProfile = worldStateUpdate.Param4;
                                break;
                            case "InitialProfile":
                                activityMemberPreset.InitialProfile = worldStateUpdate.Param4;
                                break;
                            case "EndStateProfile":
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
            List<Activity> activities = StateManagerSettings.Instance.ActivityPresets.Select(x => new Activity() { Type = x.Type }).ToList();

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
                    StartManagedService(StateManagerSettings.Instance.ActivityPresets[i].Port);
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
