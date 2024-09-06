using BotRunner.Interfaces;

namespace WoWSharpClient.Frames
{
    public class LootFrame : BotRunner.Frames.LootFrame
    {
        public IEnumerable<IWoWItem> LootItems { get; } = [];

        public LootFrame()
        {

        }
    }
}
