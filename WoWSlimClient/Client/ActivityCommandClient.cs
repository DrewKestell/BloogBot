using BaseSocketServer;
using Newtonsoft.Json;
using System.Net;
using WoWSlimClient.Models;

namespace WoWSlimClient.Client
{
    public class ActivityCommandClient(int port, IPAddress iPAddress) : AbstractSocketClient(port, iPAddress)
    {
        public bool IsRaidLeader { get; private set; }

        public ActivityMemberState GetCommandBasedOnState(ActivityMemberState characterState)
        {
            //IsRaidLeader = string.IsNullOrEmpty(characterState.RaidLeader) && characterState.RaidLeader == characterState.CharacterName;

            string json = SendMessage(JsonConvert.SerializeObject(characterState));

            return JsonConvert.DeserializeObject<ActivityMemberState>(json);
        }
    }
}
