using BotRunner.Base;
using BotRunner.Models;

namespace WoWSharpClient.Models
{
    public class Container(HighGuid highGuid) : Item( highGuid, WoWObjectType.Container)
    {
        public int NumOfSlots { get; set; }
        public IList<uint> Slots { get; internal set; } = [];
    }
}
