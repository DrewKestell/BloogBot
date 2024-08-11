using Communication;
using Microsoft.Extensions.Logging;

namespace BotCommLayer.Clients
{
    public class StateManagerUpdateClient(string ipAddress, int port, ILogger logger) : ProtobufSocketClient<DataMessage, WorldState>(ipAddress, port, logger)
    {
        public WorldState SendWorldStateUpdate(DataMessage update)
        {
            return SendMessage(update);
        }
    }
}
