using BotCommLayer;
using Communication;
using Microsoft.Extensions.Logging;

namespace BotRunner.Clients
{
    public class ActivityMemberUpdateClient(string ipAddress, int port, ILogger logger) : ProtobufSocketClient<AsyncRequest, AsyncRequest>(ipAddress, port, logger)
    {
        public AsyncRequest SendMemberStateUpdate(List<ActivitySnapshot> update)
        {
            AsyncRequest request = new();
            request.ActivitySnapshots.AddRange(update);

            return SendMessage(request);
        }
    }
}
