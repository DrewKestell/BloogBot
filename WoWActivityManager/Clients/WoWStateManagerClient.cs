using BaseSocketServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WoWActivityMember.Models;

namespace WoWActivityManager.Clients
{
    public class WoWStateManagerClient(int configPort, IPAddress ipAddress) : AbstractSocketClient(configPort, ipAddress)
    {
        public ActivityState SendCurrentActivityState(ActivityState activityState)
        {
            string json = SendMessage(JsonConvert.SerializeObject(activityState));
            return JsonConvert.DeserializeObject<ActivityState>(json);
        }
    }
}