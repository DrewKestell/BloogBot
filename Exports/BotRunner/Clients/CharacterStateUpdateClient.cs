using BotCommLayer;
using Communication;
using Microsoft.Extensions.Logging;

namespace BotRunner.Clients
{
    public class CharacterStateUpdateClient(string ipAddress, int port, ILogger logger) : ProtobufSocketClient<ActivitySnapshot, ActivitySnapshot>(ipAddress, port, logger)
    {
        public ActivitySnapshot SendMemberStateUpdate(ActivitySnapshot update) => SendMessage(update);
    }
}
