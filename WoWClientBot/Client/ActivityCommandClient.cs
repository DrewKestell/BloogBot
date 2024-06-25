using BaseSocketServer;
using Newtonsoft.Json;
using System.Net;
using WoWClientBot.Models;

namespace WoWClientBot.Client
{
    public class ActivityCommandClient(int port, IPAddress iPAddress) : AbstractSocketClient(port, iPAddress)
    {
        public bool IsRaidLeader { get; private set; }

        public CharacterCommand GetCommandBasedOnState(CharacterState characterState)
        {
            IsRaidLeader = string.IsNullOrEmpty(characterState.RaidLeader) && characterState.RaidLeader == characterState.CharacterName;

            string json = SendMessage(JsonConvert.SerializeObject(characterState));

            return JsonConvert.DeserializeObject<CharacterCommand>(json);
        }
    }
}
