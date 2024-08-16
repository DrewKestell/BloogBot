using BotRunner.Constants;
using System.Net;
using WoWSharpClient.Manager;
using WoWSharpClient.Models;

namespace WoWSharpClient.Client
{
    public class WoWClient(string ipAddress, int port, WoWSharpEventEmitter woWSharpEventEmitter, ObjectManager objectManager) : IDisposable
    {
        private WorldClient _worldClient;
        private readonly LoginClient _loginClient = new(ipAddress, port, woWSharpEventEmitter, objectManager);
        private readonly IPAddress _ipAddress = IPAddress.Parse(ipAddress);
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter = woWSharpEventEmitter;
        private readonly ObjectManager _objectManager = objectManager;

        public void ConnectToLogin()
        {
            _loginClient.Connect();
            _woWSharpEventEmitter.FireOnLoginConnect();
        }

        public void Login(string username, string password)
        {
            try
            {
                if (!_objectManager.IsLoginConnected)
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

        private void Instance_OnPlayerInit(object? sender, EventArgs e)
        {
            StartServerPing();
        }

        public List<Realm> GetRealmList()
        {
            return _loginClient.GetRealmList();
        }

        public void SelectRealm(Realm realm)
        {
            _worldClient = new WorldClient(_woWSharpEventEmitter, _objectManager);
            _objectManager.CurrentRealm = realm;

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

        private void StartServerPing()
        {
            _worldClient.StartServerPing();
        }

        public void SendChatMessage(ChatMsg chatMsgType, Language orcish, string destination, string text)
        {
            _worldClient.SendCMSGMessageChat(chatMsgType, orcish, destination, text);
        }

        public void SendNameQuery(ulong guid)
        {
            _worldClient.SendCMSGTypeQuery(Opcodes.CMSG_NAME_QUERY, guid);
        }

        public void Dispose()
        {
            _loginClient?.Dispose();
            _worldClient?.Dispose();
        }
    }
}
