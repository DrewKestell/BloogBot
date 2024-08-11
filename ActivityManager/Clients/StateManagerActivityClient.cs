using BotCommLayer;
using Communication;

namespace ActivityManager.Clients
{
    public class StateManagerActivityClient(string ipAddress, int configPort, ILogger logger) : ProtobufSocketClient<DataMessage, Activity>(ipAddress, configPort, logger)
    {
        public Activity UpdateCurrentActivityState(Activity activity) => SendMessage(new DataMessage() { Activity = activity });
    }
}