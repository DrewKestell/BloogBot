using BaseSocketServer;
using System.Net;

namespace WoWActivityManager.Client
{
    public class WoWStateManagerClient(int port, IPAddress iPAddress) : AbstractSocketClient(port, iPAddress)
    {

    }
}
