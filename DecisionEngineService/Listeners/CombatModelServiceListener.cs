using BotCommLayer;
using Communication;

namespace DecisionEngineService.Listeners
{
    public class CombatModelServiceListener(string ipAddress, int port, ILogger logger) : ProtobufSocketServer<ActivitySnapshot, ActivitySnapshot>(ipAddress, port, logger)
    {
        protected override ActivitySnapshot HandleRequest(ActivitySnapshot request)
        {
            return base.HandleRequest(request);
        }
    }
}
