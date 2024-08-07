using BotCommLayer;
using Communication;

namespace StateManager.Clients
{
    public class StateManagerUpdateClient(string ipAddress, int port) : ProtobufSocketClient<WorldStateUpdate, WorldState>(ipAddress, port)
    {
        public WorldState SendWorldStateUpdate(WorldStateUpdate update)
        {
            return SendMessage(update);
        }
    }
}
