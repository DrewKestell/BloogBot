using BotCommLayer;
using Communication;

namespace StateManager.Clients
{
    public class ActivityMemberUpdateClient(string ipAddress, int port, ILogger logger) : ProtobufSocketClient<ActivitySnapshot, ActivitySnapshot>(ipAddress, port, logger)
    {
        public ActivitySnapshot SendMemberStateUpdate(ActivitySnapshot update)
        {
            return SendMessage(update);
        }
    }
}
