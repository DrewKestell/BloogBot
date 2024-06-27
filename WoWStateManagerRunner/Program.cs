using System.Net;
using WoWActivityMember.Models;
using WoWStateManager;
using WoWStateManager.Models;

namespace WoWStateManagerRunner
{
    public class Program
    {
        static WorldStateManagerServer WorldStateManagerServer;
        static WorldStateActivityServer WorldStateActivityServer;
        public static void Main(string[] args)
        {

            WorldStateManagerServer = new WorldStateManagerServer(8089, IPAddress.Parse("127.0.0.1"));
            WorldStateActivityServer = new WorldStateActivityServer(8090, IPAddress.Parse("127.0.0.1"));

            WorldStateManagerServer.InstanceUpdateObservable.Subscribe(OnInstanceUpdate);

            WorldStateManagerServer.Start();
            WorldStateActivityServer.Start();

            Console.WriteLine($"Exit at anytime by pressing a button");
            Console.ReadKey();
        }

        private static void OnInstanceUpdate(ActivityCommand activityCommand)
        {
            if (activityCommand.ActivityAction != ActivityAction.None)
            {
                Console.WriteLine($"{DateTime.Now}| [Processing Activity] {activityCommand.ActivityAction} {activityCommand.CommandParam1} {activityCommand.CommandParam2} {activityCommand.CommandParam3} {activityCommand.CommandParam4}");

                int activityIndex = int.Parse(activityCommand.CommandParam1);
                int activityMemberIndex = int.Parse(activityCommand.CommandParam2);
                switch (activityCommand.ActivityAction)
                {
                    case ActivityAction.AddActivity:
                        WoWStateManagerSettings.Instance.ActivityPresets.Add(new ActivityPreset());
                        break;
                    case ActivityAction.EditActivity:
                        if (activityCommand.CommandParam3 == "Remove")
                            WoWStateManagerSettings.Instance.ActivityPresets.RemoveAt(activityIndex);
                        else
                        {
                            WoWStateManagerSettings.Instance.ActivityPresets[activityIndex].ActivityType = (ActivityType)Enum.Parse(typeof(ActivityType), activityCommand.CommandParam3);

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

                        switch (activityCommand.CommandParam3)
                        {
                            case "BehaviorProfile":
                                activityMemberPreset.BehaviorProfile = activityCommand.CommandParam4;
                                break;
                            case "Account":
                                activityMemberPreset.Account = activityCommand.CommandParam4;
                                break;
                            case "ProgressionConfig":
                                activityMemberPreset.ProgressionConfig = activityCommand.CommandParam4;
                                break;
                            case "InitialStateConfig":
                                activityMemberPreset.InitialStateConfig = activityCommand.CommandParam4;
                                break;
                            case "EndStateConfig":
                                activityMemberPreset.EndStateConfig = activityCommand.CommandParam4;
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
                }
            }
            WorldStateManagerServer.SendCommandToProcess(activityCommand.ProcessId,
                WoWStateManagerSettings.Instance.ActivityPresets.Select(x => new ActivityState()
                {
                    ProcessId = WoWStateManagerSettings.Instance.ActivityPresets.IndexOf(x) + 1,
                    IsConnected = false,
                    ShouldRun = false,
                    ActivityType = x.ActivityType,
                    ActivityMemberPresets = x.ActivityMemberPresets
                }).ToList());
        }
    }
}