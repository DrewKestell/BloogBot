using BotRunner.Base;
using BotRunner.Interfaces;
using BotRunner.Models;

namespace WoWSharpClient.Models
{
    public class Object(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.None) : BaseWoWObject(0, highGuid, objectType), IWoWObject
    {
        public string Name { get; set; } = string.Empty;
        protected override nint GetDescriptorPtr()
        {
            return 0;
        }
    }
}
