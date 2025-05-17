namespace GameData.Core.Interfaces
{
    public interface IWoWContainer : IWoWItem
    {
        int NumOfSlots { get; }
        uint[] Slots { get; }
        ulong GetItemGuid(int parSlot);
    }
}
