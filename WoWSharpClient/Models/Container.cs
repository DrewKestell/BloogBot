using BotRunner.Interfaces;

namespace WoWSharpClient.Models
{
    public class Container(byte[] lowGuid, byte[] highGuid) : Item(lowGuid, highGuid, WoWObjectType.Container)
    {
        public int Slots { get; set; }
    }
}
