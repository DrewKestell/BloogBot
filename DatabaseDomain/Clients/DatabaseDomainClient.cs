using BotCommLayer;
using Database;

namespace DatabaseDomain.Clients
{
    public class DatabaseDomainClient(string ipAddress, ILogger logger) : ProtobufSocketClient<DatabaseRequest, DatabaseResponse>(ipAddress, 8080, logger)
    {

    }
}
