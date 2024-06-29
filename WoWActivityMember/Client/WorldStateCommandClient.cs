using BaseSocketServer;
using Newtonsoft.Json;
using System.Net;
using WoWActivityMember.Models;

namespace WoWActivityMember.Client
{
    public class WorldStateCommandClient(int port, IPAddress iPAddress) : AbstractSocketClient(port, iPAddress)
    {
        public List<ActivityState> SendActivityRequest(WorldStateUpdate activityCommand)
        {
            return JsonConvert.DeserializeObject<List<ActivityState>>(SendMessage(JsonConvert.SerializeObject(activityCommand)));
        }
    }
}
