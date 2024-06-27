using BaseSocketServer;
using Newtonsoft.Json;
using System.Net;
using WoWActivityMember.Models;

namespace WoWActivityMember.Client
{
    public class ActivityCommandClient(int port, IPAddress iPAddress) : AbstractSocketClient(port, iPAddress)
    {
        public bool IsRaidLeader { get; private set; }

        public CharacterState GetCommandBasedOnState(CharacterState characterState)
        {
            //IsRaidLeader = string.IsNullOrEmpty(characterState.RaidLeader) && characterState.RaidLeader == characterState.CharacterName;

            string json = SendMessage(JsonConvert.SerializeObject(characterState));

            return JsonConvert.DeserializeObject<CharacterState>(json);
        }
    }
}
