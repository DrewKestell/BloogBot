using GameData.Core.Enums;
using GameData.Core.Models;
using System.Net;

namespace WoWSharpClient.Client
{
    public class WoWClient(string ipAddress, WoWSharpObjectManager woWSharpObjectManager) : IDisposable
    {
        private readonly WorldClient _worldClient = new(woWSharpObjectManager.EventEmitter, new(woWSharpObjectManager));
        private readonly AuthLoginClient _loginClient = new(ipAddress, woWSharpObjectManager.EventEmitter);
        private readonly IPAddress _ipAddress = IPAddress.Parse(ipAddress);
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter = woWSharpObjectManager.EventEmitter;
        private readonly WoWSharpObjectManager _objectManager = woWSharpObjectManager;

        private bool _isLoggedIn;
        public bool IsLoggedIn => _isLoggedIn;

        public void Dispose()
        {
            _loginClient?.Dispose();
            _worldClient?.Dispose();
        }

        public bool IsLoginConnected() => _loginClient.IsConnected;
        public bool IsWorldConnected() => _worldClient.IsConnected;

        private void ConnectToLoginServer()
        {
            _loginClient.Connect();
            _woWSharpEventEmitter.FireOnLoginConnect();
        }

        public void Login(string username, string password)
        {
            try
            {
                if (!_loginClient.IsConnected)
                {
                    ConnectToLoginServer();
                }

                _loginClient.Login(username, password);
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Exception occured during login: {ex}");
                _isLoggedIn = false;
            }
            _isLoggedIn = true;
        }

        private void Instance_OnPlayerInit(object? sender, EventArgs e) => StartServerPing();

        public List<Realm> GetRealmList() => _loginClient.GetRealmList();
        public void SelectRealm(Realm realm) => _worldClient.Connect(_loginClient.Username, _ipAddress, _loginClient.SessionKey, realm.AddressPort);

        public void RefreshCharacterSelects() => _worldClient.SendCMSGCharEnum();
        public void SendCharacterCreate(string name, Race race, Class clazz, Gender gender, byte skin, byte face, byte hairStyle, byte hairColor, byte facialHair)
            => _worldClient.SendCMSGCreateCharacter(name, race, clazz, gender, skin, face, hairStyle, hairColor, facialHair);
        public void EnterWorld(ulong guid) => _worldClient.SendCMSGPlayerLogin(guid);
        private void StartServerPing() => _worldClient.StartServerPing();
        public void SendChatMessage(ChatMsg chatMsgType, Language orcish, string destination, string text) => _worldClient.SendCMSGMessageChat(chatMsgType, orcish, destination, text);
        public void SendNameQuery(ulong guid) => _worldClient.SendCMSGTypeQuery(Opcode.CMSG_NAME_QUERY, guid);
    }
}
