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
        public WorldStateManagerServer()
        {
            WorldStateManagerSocketListener = new WorldStateManagerSocketListener();
            WorldStateActivitySocketListener = new WorldStateActivitySocketListener();

            CurrentActivityList = WoWStateManagerSettings.Instance.ActivityPresets.Select(x => new ActivityState()
            {
                ActivityType = x.ActivityType,
                ActivityMemberStates = x.ActivityMemberPresets,
            }).ToList();

            WorldStateManagerSocketListener.InstanceUpdateObservable.Subscribe(OnWorldStateUpdate);
            WorldStateActivitySocketListener.InstanceUpdateObservable.Subscribe(OnActivityManagerUpdate);

            WorldStateManagerSocketListener.Start();
            WorldStateActivitySocketListener.Start();
        }

        private void OnActivityManagerUpdate(ActivityState state)
        {
            if (CurrentActivityList.Any(x => x.ProcessId == state.ProcessId))
            {
                WorldStateActivitySocketListener.SendCommandToProcess(state.ProcessId,
                    CurrentActivityList.First(x => x.ProcessId == state.ProcessId));
            }
            else if (CurrentActivityList.Any(x => x.ProcessId == 0))
            {
                ActivityState activityState = CurrentActivityList.First(x => x.ProcessId == 0);
                activityState.ProcessId = state.ProcessId;

                WorldStateActivitySocketListener.SendCommandToProcess(state.ProcessId,
                    activityState);
            }
            else
            {
                WorldStateActivitySocketListener.SendCommandToProcess(state.ProcessId,
                    new ActivityState() { ActivityType = ActivityType.Idle });
            }
        }

        private void OnWorldStateUpdate(WorldStateUpdate worldStateUpdate)
        {
            if (worldStateUpdate.ActivityAction != ActivityAction.None)
            {
                Console.WriteLine($"{DateTime.Now}|[WOW STATE MANAGER RUNNER]Processing Activity - {worldStateUpdate.ActivityAction} {worldStateUpdate.CommandParam1} {worldStateUpdate.CommandParam2} {worldStateUpdate.CommandParam3} {worldStateUpdate.CommandParam4}");

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

                        for (int i = 0; i < WoWStateManagerSettings.Instance.ActivityPresets.Count; i++)
                            if (CurrentActivityList.Count < i)
                                CurrentActivityList.Add(new ActivityState()
                                {
                                    ActivityType = WoWStateManagerSettings.Instance.ActivityPresets[i].ActivityType,
                                    ActivityMemberStates = WoWStateManagerSettings.Instance.ActivityPresets[i].ActivityMemberPresets
                                });
                            else
                            {
                                CurrentActivityList[i].ActivityType = WoWStateManagerSettings.Instance.ActivityPresets[i].ActivityType;
                                CurrentActivityList[i].ActivityMemberStates = WoWStateManagerSettings.Instance.ActivityPresets[i].ActivityMemberPresets;
                            }

                        if (CurrentActivityList.Count > WoWStateManagerSettings.Instance.ActivityPresets.Count)
                            CurrentActivityList.RemoveRange(WoWStateManagerSettings.Instance.ActivityPresets.Count, CurrentActivityList.Count - WoWStateManagerSettings.Instance.ActivityPresets.Count);

                        break;
                }
            }
            WorldStateManagerSocketListener.SendCommandToProcess(worldStateUpdate.ProcessId,
                WoWStateManagerSettings.Instance.ActivityPresets.Select(x => new ActivityState()
                {
                    ProcessId = WoWStateManagerSettings.Instance.ActivityPresets.IndexOf(x) + 1 > CurrentActivityList.Count ? 0 : CurrentActivityList[WoWStateManagerSettings.Instance.ActivityPresets.IndexOf(x)].ProcessId,
                    ActivityType = x.ActivityType,
                    ActivityMemberStates = x.ActivityMemberPresets
                }).ToList());
        }
    }
}