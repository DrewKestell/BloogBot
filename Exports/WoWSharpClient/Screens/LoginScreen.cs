using GameData.Core.Enums;
using GameData.Core.Frames;
using GameData.Core.Models;
using WoWSharpClient.Client;

namespace WoWSharpClient.Screens
{
    public class LoginScreen(WoWClient woWClient) : ILoginScreen
    {
        public bool IsOpen => true;
        public uint QueuePosition => 0;
        public bool IsLoggedIn => woWClient.IsLoggedIn;

        public void CancelLogin()
        {

        }

        public void Login(string username, string password) => woWClient.Login(username, password);
    }

    public class RealmSelectScreen(WoWClient woWClient) : IRealmSelectScreen
    {
        public bool IsOpen => true;
        public Realm? CurrentRealm { get; private set; }

        public void CancelRealmSelection()
        {

        }

        public List<Realm> GetRealmList() => woWClient.GetRealmList();
        public void SelectRealm(Realm realm)
        {
            woWClient?.SelectRealm(realm);
            CurrentRealm = realm;
        }

        public void SelectRealmType(RealmType realmType)
        {

        }
    }

    public class CharacterSelectScreen(WoWClient woWClient) : ICharacterSelectScreen
    {
        public bool IsOpen => true;
        public bool HasReceivedCharacterList { get; set; }
        public bool HasRequestedCharacterList { get; set; }
        public void RefreshCharacterListFromServer()
        {
            woWClient.RefreshCharacterSelects();
            HasRequestedCharacterList = true;
            HasReceivedCharacterList = false;
        }
        public void CreateCharacter(string name, Race race, Gender gender, Class @class, byte skinColor, byte face, byte hairStyle, byte hairColor, byte facialHair, byte outfitId)
        {
            woWClient.SendCharacterCreate(name, race, @class, gender, skinColor, face, hairStyle, hairColor, facialHair, outfitId);
            HasRequestedCharacterList = false;
            HasReceivedCharacterList = false;
        }

        public void DeleteCharacter(ulong characterGuid)
        {

        }
        public List<CharacterSelect> CharacterSelects { get; } = [];
    }
}
