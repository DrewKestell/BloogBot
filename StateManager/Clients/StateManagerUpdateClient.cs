using BotCommLayer;
using Communication;

namespace StateManager.Clients
{
    public class StateManagerUpdateClient(string ipAddress, int port, ILogger logger) : ProtobufSocketClient<DataMessage, WorldState>(ipAddress, port, logger)
    {
        public WorldState SendWorldStateUpdate(WorldStateUpdate update)
        {            
            return SendMessage(new DataMessage() { Id = 101, WorldStateUpdate = update });
        }
    }
}
