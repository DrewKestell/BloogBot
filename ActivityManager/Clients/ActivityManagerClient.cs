using BotCommLayer;
using Communication;

namespace ActivityManager.Clients
{
    public class ActivityManagerClient(string ipAddress, int configPort) : ProtobufSocketClient<ActivityMemberState, ActivityMember>(ipAddress, configPort)
    {
        public ActivityMember SendCurrentStateToActivityManager(ActivityMemberState currentActivityState)
        {
            return SendMessage(currentActivityState);
        }
    }
}
