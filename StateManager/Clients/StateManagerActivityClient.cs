using BotCommLayer;
using Communication;

namespace StateManager.Clients
{
    public class StateManagerActivityClient(string ipAddress, int configPort, ILogger logger) : ProtobufSocketClient<Activity, Activity>(ipAddress, configPort, logger) { }
}