using System.Net;
using WoWSlimClient.Manager;
using WoWSlimClient.Models;

namespace WoWSlimClient.Client
{
    public class WoWClient
    {
        private WorldClient _worldClient;
        private LoginClient _loginClient;

        private IPAddress _ipAddress;


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

                if (!ObjectManager.Instance.IsLoginConnected)
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
            ObjectManager.Instance.CurrentRealm = realm;

            _worldClient.Connect(_loginClient.Username, _ipAddress, _loginClient.SessionKey, realm.AddressPort);
        }

        public void RefreshCharacterSelects()
        {
            _worldClient.SendCMSGCharEnum();
        }
        public void EnterWorld(ulong guid)
        {
            _worldClient.SendCMSGPlayerLogin(guid);
        }
    }
}
