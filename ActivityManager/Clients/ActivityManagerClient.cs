using BotCommLayer;
using Communication;

namespace ActivityManager.Clients
{
    public class ActivityManagerClient(string ipAddress, int port, ILogger logger) : ProtobufSocketClient<ActivityMemberState, ActivityMember>(ipAddress, port, logger)
    {
        public ActivityMember SendCurrentStateToActivityManager(ActivityMemberState currentActivityState)
        {
            return SendMessage(currentActivityState);
        }
    }
}
