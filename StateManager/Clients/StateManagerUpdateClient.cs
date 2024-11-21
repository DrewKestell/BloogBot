using BotCommLayer;
using Communication;

namespace StateManager.Clients
{
    public class StateManagerUpdateClient(string ipAddress, int port, ILogger logger) : ProtobufSocketClient<ActivitySnapshot, ActivitySnapshot>(ipAddress, port, logger)
    {
        public ActivitySnapshot SendWorldStateUpdate(ActivitySnapshot update)
        {
            return SendMessage(update);
        }
    }
}
