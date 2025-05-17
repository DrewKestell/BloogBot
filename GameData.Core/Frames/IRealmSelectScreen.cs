using GameData.Core.Models;

namespace GameData.Core.Frames
{
    public interface IRealmSelectScreen
    {
        bool IsOpen { get; }
        Realm? CurrentRealm { get; }
        List<Realm> GetRealmList();
        void SelectRealmType(RealmType realmType);
        void SelectRealm(Realm realm);
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
