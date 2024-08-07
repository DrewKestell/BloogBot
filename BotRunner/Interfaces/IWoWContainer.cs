namespace BotRunner.Interfaces
{
    public interface IWoWContainer : IWoWItem
    {
        int Slots { get; }

        ulong GetItemGuid(int parSlot);
    }
}
