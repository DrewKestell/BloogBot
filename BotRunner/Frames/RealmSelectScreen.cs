namespace BotRunner.Frames
{
    public interface IRealmSelectScreen
    {
        bool IsOpen { get; }
        void SelectRealmType(RealmType realmType);
        void SelectRealm(int realmIndex);
        void CancelRealmSelection();
    }
    public enum RealmType
    {
        PvE,
        PvP,
        RPPvE,
        RPPvP
    }
}
