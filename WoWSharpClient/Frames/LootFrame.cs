using BotRunner.Interfaces;

namespace WoWSharpClient.Frames
{
    public class LootFrame : ILootFrame
    {
        public IEnumerable<IWoWItem> LootItems { get; } = [];

        public LootFrame()
        {
            
        }
    }
}
