using Communication;
using Microsoft.Extensions.Logging;

namespace BotCommLayer.Clients
{
    public class ActivityMemberUpdateClient(string ipAddress, int port, ILogger logger) : ProtobufSocketClient<ActivityMemberState, ActivityMember>(ipAddress, port, logger)
    {
        public ActivityMember SendMemberStateUpdate(ActivityMemberState update)
        {
            return SendMessage(update);
        }
    }
}
