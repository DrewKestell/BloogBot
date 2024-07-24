
namespace WoWSlimClient.Models
{
    public class WoWContainer(byte[] lowGuid, byte[] highGuid) : WoWItem(lowGuid, highGuid, WoWObjectType.Container)
    {
        public int Slots { get; set; }
    }
}
