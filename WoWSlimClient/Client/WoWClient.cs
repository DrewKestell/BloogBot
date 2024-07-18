using System.Net;
using WoWSlimClient.Models;

namespace WoWSlimClient.Client
{
    public class WoWClient
    {
        private WorldClient _worldClient;
        private LoginClient _loginClient;

        private IPAddress _ipAddress;

        private Realm _currentRealm;

        public bool IsLoggedIn => _loginClient.IsLoggedIn;
        public bool HasRealmSelected => _currentRealm != null;
        public bool HasEnteredWorld => _worldClient.HasEnteredWorld;

        public WoWClient(string ipAddress, int port = 3724)
        {
            _ipAddress = IPAddress.Parse(ipAddress);
            _loginClient = new LoginClient(ipAddress, port);
        }

        public void Login(string username, string password)
        {
            try
            {
                _loginClient.Connect();

                if (!_loginClient.IsConnected)
                {
                    throw new Exception("Unable to login to WoW server");
                }

                _loginClient.Login(username, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during login: {ex}");
            }
        }

        public List<Realm> GetRealmList()
        {
            return _loginClient.GetRealmList();
        }

        public void SelectRealm(Realm realm)
        {
            _worldClient = new WorldClient();
            _currentRealm = realm;

            _worldClient.Connect(_loginClient.Username, _ipAddress, _loginClient.SessionKey, realm.AddressPort);
        }

        public List<CharacterSelect> GetCharacterListFromRealm()
        {
            return _worldClient.GetCharactersOnRealm();
        }
        public void EnterWorld(ulong guid)
        {
            _worldClient.EnterWorld(guid);
        }
    }
}
