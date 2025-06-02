using BotCommLayer;
using Communication;

namespace DecisionEngineService.Clients
{
    public class CombatModelClient(string ipAddress, ILogger logger) : ProtobufSocketClient<ActivitySnapshot, ActivitySnapshot>(ipAddress, 8080, logger)
    {

    }
}
