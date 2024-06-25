using BaseSocketServer;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using WoWClientBot.Models;

namespace WoWActivityManager
{
    public class ActivityManagerSocketServer(int port, IPAddress ipAddress) : AbstractSocketServer(port, ipAddress)
    {
        private readonly Subject<CharacterState> _instanceUpdateSubject = new();

        public IObservable<CharacterState> InstanceUpdateObservable => _instanceUpdateSubject;

        public override int HandleRequest(string payload, Socket clientSocket)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return 0;
            }
            CharacterState instanceUpdate = JsonConvert.DeserializeObject<CharacterState>(payload);
            int processId = instanceUpdate.ProcessId;

            if (processId != 0)
            {
                if (!_processIds.ContainsKey(processId))
                {
                    Console.WriteLine($"[ACTIVITY MANAGER SERVER : {_port}]Process connected {processId}");
                    _processIds.Add(processId, clientSocket);
                }

                instanceUpdate.IsConnected = true;
                _instanceUpdateSubject.OnNext(instanceUpdate);
            }
            return processId;
        }

        public bool SendCommandToProcess(int processId, CharacterCommand instanceCommand)
        {
            if (_processIds.ContainsKey(processId) && _processIds.TryGetValue(processId, out Socket clientSocket))
            {
               SendMessage(JsonConvert.SerializeObject(instanceCommand), clientSocket);
            }
            return false;
        }
    }
}
