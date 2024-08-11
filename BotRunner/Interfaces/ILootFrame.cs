namespace BotRunner.Interfaces
{
    public interface ILootFrame
    {
        IEnumerable<IWoWItem> LootItems { get; }
    }
}
