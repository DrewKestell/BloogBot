using BotCommLayer;
using Communication;

namespace StateManager.Clients
{
    public class StateManagerUpdateClient(string ipAddress, int port, ILogger logger) : ProtobufSocketClient<StateChangeRequest, StateChangeResponse>(ipAddress, port, logger)
    {
        public StateChangeResponse SendWorldStateUpdate(StateChangeRequest update) => SendMessage(update);
    }
}
