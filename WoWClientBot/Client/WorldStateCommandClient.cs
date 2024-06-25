using BaseSocketServer;
using Newtonsoft.Json;
using System.Net;
using WoWClientBot.Models;

namespace WoWClientBot.Client
{
    public class WorldStateCommandClient(int port, IPAddress iPAddress) : AbstractSocketClient(port, iPAddress)
    {
        public List<ActivityState> SendActivityRequest(ActivityCommand activityCommand)
        {
            return JsonConvert.DeserializeObject<List<ActivityState>>(SendMessage(JsonConvert.SerializeObject(activityCommand)));
        }
    }
}
