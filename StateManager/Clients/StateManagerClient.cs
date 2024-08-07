using BotCommLayer;
using Communication;

namespace StateManager.Clients
{
    public class StateManagerClient(string ipAddress, int configPort) : ProtobufSocketClient<Activity, Activity>(ipAddress, configPort) { }
}