using System.Net;
using System.Net.Sockets;
using WoWActivityManager;
using WoWActivityMember.Models;
using WoWStateManager.Listeners;
using WoWStateManager.Models;

namespace WoWStateManager
{
    public class WorldStateManagerServer
    {
        private readonly WorldStateManagerSocketListener WorldStateManagerSocketListener;
        private readonly WorldStateActivitySocketListener WorldStateActivitySocketListener;
        private readonly List<ActivityState> CurrentActivityList;
        private readonly List<WoWActivityManagerServer> ActivityManagers;
        private readonly CancellationTokenSource CancellationTokenSource = new();
        private int MaxAllowedClients => CurrentActivityList.SelectMany(x => x.ActivityMemberStates).Count();

        public WorldStateManagerServer()
        {
            WorldStateManagerSocketListener = new WorldStateManagerSocketListener();
            WorldStateActivitySocketListener = new WorldStateActivitySocketListener();

            CurrentActivityList = [];
            ActivityManagers = [];

            WorldStateManagerSocketListener.InstanceUpdateObservable.Subscribe(OnWorldStateUpdate);
            WorldStateActivitySocketListener.InstanceUpdateObservable.Subscribe(OnActivityManagerUpdate);
        }

        public void Start()
        {
            //Host.CreateDefaultBuilder()
            //    .UseWindowsService(options =>
            //    {
            //        options.ServiceName = "WoW Activity Manager";
            //    })
            //    .ConfigureServices((hostContext, services) =>
            //    {

            //    })
            //    .Build();

            WorldStateManagerSocketListener?.Start();
            WorldStateActivitySocketListener?.Start();
        }

        private void OnActivityManagerUpdate(ActivityState state)
        {
            if (CurrentActivityList.Any(x => x.ServiceId == state.ServiceId))
            {
                WorldStateActivitySocketListener.SendCommandToProcess(state.ServiceId,
                    CurrentActivityList.First(x => x.ServiceId == state.ServiceId));
            }
            else if (CurrentActivityList.Any(x => x.ServiceId == Guid.Empty))
            {
                ActivityState activityState = CurrentActivityList.First(x => x.ServiceId == Guid.Empty);
                activityState.ServiceId = state.ServiceId;

                WorldStateActivitySocketListener.SendCommandToProcess(state.ServiceId,
                    activityState);
            }
            else
            {
                WorldStateActivitySocketListener.SendCommandToProcess(state.ServiceId,
                    new ActivityState()
                    {
                        ActivityType = ActivityType.Idle,
                        MaxAllowedClients = 0
                    });
            }
        }

        private void OnWorldStateUpdate(WorldStateUpdate worldStateUpdate)
        {
            
            if (worldStateUpdate.ActivityAction != ActivityAction.None)
            {
                Console.WriteLine($"{DateTime.Now}|[WorldStateManagerServer]Processing {worldStateUpdate.ActivityAction} {worldStateUpdate.CommandParam1} {worldStateUpdate.CommandParam2} {worldStateUpdate.CommandParam3} {worldStateUpdate.CommandParam4}");

                int activityIndex = int.Parse(worldStateUpdate.CommandParam1);
                int activityMemberIndex = int.Parse(worldStateUpdate.CommandParam2);
                switch (worldStateUpdate.ActivityAction)
                {
                    case ActivityAction.AddActivity:
                        WoWStateManagerSettings.Instance.ActivityPresets.Add(new ActivityPreset());
                        break;
                    case ActivityAction.EditActivity:
                        if (worldStateUpdate.CommandParam3 == "Remove")
                            WoWStateManagerSettings.Instance.ActivityPresets.RemoveAt(activityIndex);
                        else
                        {
                            WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].ActivityType = (ActivityType)Enum.Parse(typeof(ActivityType), worldStateUpdate.CommandParam3);

                            while (WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].MinActivitySize > WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].ActivityMemberPresets.Count)
                                WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].ActivityMemberPresets.Add(new ActivityMemberPreset());

                            while (WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].MaxActivitySize < WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].ActivityMemberPresets.Count)
                                WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].ActivityMemberPresets.RemoveAt(WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].ActivityMemberPresets.Count - 1);
                        }
                        break;
                    case ActivityAction.AddActivityMember:
                        WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].ActivityMemberPresets.Add(new ActivityMemberPreset());
                        break;
                    case ActivityAction.EditActivityMember:
                        ActivityPreset activityPreset = WoWStateManagerSettings.Instance.ActivityPresets[activityIndex];
                        ActivityMemberPreset activityMemberPreset = activityPreset.ActivityMemberPresets[activityMemberIndex];

                        switch (worldStateUpdate.CommandParam3)
                        {
                            case "BehaviorProfile":
                                activityMemberPreset.BehaviorProfile = worldStateUpdate.CommandParam4;
                                break;
                            case "Account":
                                activityMemberPreset.Account = worldStateUpdate.CommandParam4;
                                break;
                            case "ProgressionConfig":
                                activityMemberPreset.ProgressionConfig = worldStateUpdate.CommandParam4;
                                break;
                            case "InitialStateConfig":
                                activityMemberPreset.InitialStateConfig = worldStateUpdate.CommandParam4;
                                break;
                            case "EndStateConfig":
                                activityMemberPreset.EndStateConfig = worldStateUpdate.CommandParam4;
                                break;
                            case "Remove":
                                activityPreset.ActivityMemberPresets.RemoveAt(activityMemberIndex);
                                break;
                        }
                        break;
                    case ActivityAction.SetMaxMemberSize:
                        while (WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].MaxActivitySize > WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].ActivityMemberPresets.Count)
                            WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].ActivityMemberPresets.Add(new ActivityMemberPreset());
                        break;
                    case ActivityAction.SetMinMemberSize:
                        while (WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].MinActivitySize < WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].ActivityMemberPresets.Count)
                            WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].ActivityMemberPresets.RemoveAt(WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].ActivityMemberPresets.Count - 1);
                        break;
                    case ActivityAction.ApplyDesiredState:
                        WoWStateManagerSettings.Instance.SaveConfig();

                        ApplyDesiredState();
                        break;
                }
            }

            foreach (WoWActivityManagerServer woWActivityManagerServer in ActivityManagers)
                woWActivityManagerServer.UpdateCurrentState(CancellationTokenSource.Token);

            WorldStateManagerSocketListener.SendCommandToProcess(worldStateUpdate.ProcessId,
                WoWStateManagerSettings.Instance.ActivityPresets.Select(x => new ActivityState()
                {
                    ActivityType = x.ActivityType,
                    ActivityMemberStates = x.ActivityMemberPresets
                }).ToList());
        }

        private void ApplyDesiredState()
        {
            for (int i = 0; i < WoWStateManagerSettings.Instance.ActivityPresets.Count; i++)
            {
                if (CurrentActivityList.Count < WoWStateManagerSettings.Instance.ActivityPresets.Count)
                {
                    CurrentActivityList.Add(new()
                    {
                        ActivityType = WoWStateManagerSettings.Instance.ActivityPresets[i].ActivityType,
                        ActivityMemberStates = WoWStateManagerSettings.Instance.ActivityPresets[i].ActivityMemberPresets
                    });

#if DEBUG
                    ActivityManagers.Add(new WoWActivityManagerServer(IPAddress.Loopback, FreeTcpPort(), IPAddress.Loopback, 8089));
#else
                    //host.Run();
#endif

                }
                else
                {
                    CurrentActivityList[i].ActivityType = WoWStateManagerSettings.Instance.ActivityPresets[i].ActivityType;
                    CurrentActivityList[i].ActivityMemberStates = WoWStateManagerSettings.Instance.ActivityPresets[i].ActivityMemberPresets;
                }
            }

            while (CurrentActivityList.Count > WoWStateManagerSettings.Instance.ActivityPresets.Count)
            {
#if DEBUG
                ActivityManagers.RemoveAt(CurrentActivityList.Count - 1);
#else

#endif
                CurrentActivityList.RemoveAt(CurrentActivityList.Count - 1);
            }

            CurrentActivityList.ForEach(x => x.MaxAllowedClients = MaxAllowedClients);
        }
        static int FreeTcpPort()
        {
            Socket testSocket;

            for (int i = 8090; i < 8190; i++)
            {
                try
                {
                    testSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    testSocket.Bind(new IPEndPoint(IPAddress.Loopback, i));
                    testSocket.Close();
                    return i;
                }
                catch (Exception)
                {
                }
            }

            return 0;
        }
    }
}