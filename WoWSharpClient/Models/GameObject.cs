using BotRunner.Base;
using BotRunner.Interfaces;
using BotRunner.Models;

namespace WoWSharpClient.Models
{
    public class GameObject(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.GameObj) : Object(highGuid, objectType), IWoWGameObject
    {
        public GOState GoState { get; set; }
        public uint ArtKit { get; set; }
        public uint AnimProgress { get; set; }
        public uint Level { get; set; }
        public uint FactionTemplate { get; set; }
        public uint TypeId { get; set; }
        public HighGuid Owner { get; internal set; } = new HighGuid(new byte[4], new byte[4]);
        public uint Flags { get; internal set; }
        public uint Faction { get; internal set; }
        public float[] Rotation { get; set; } = new float[4];

        public void Interact()
        {

        }

        protected override nint GetDescriptorPtr()
        {
            return 0;
        }
    }
}
