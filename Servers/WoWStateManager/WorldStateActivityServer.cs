using BaseSocketServer;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using WoWClientBot.Models;

namespace WoWStateManager
{
    public class WorldStateActivityServer(int port, IPAddress ipAddress) : AbstractSocketServer(port, ipAddress)
    {
        private readonly Subject<ActivityState> _instanceUpdateSubject = new();

        public IObservable<ActivityState> InstanceUpdateObservable => _instanceUpdateSubject;

        public override int HandleRequest(string payload, Socket clientSocket)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return 0;
            }
            ActivityState instanceUpdate = JsonConvert.DeserializeObject<ActivityState>(payload);
            int processId = instanceUpdate.ProcessId;

            if (processId != 0)
            {
                if (!_processIds.ContainsKey(processId))
                {
                    Console.WriteLine($"[ACTIVITY MANAGER SERVER {_port}]Process connected {processId}");
                    _processIds.Add(processId, clientSocket);
                }

                instanceUpdate.IsConnected = true;
                _instanceUpdateSubject.OnNext(instanceUpdate);
            }
            return processId;
        }

        public bool SendCommandToProcess(int processId, CharacterCommand instanceCommand)
        {
            if (_processIds.ContainsKey(processId))
            {
                _processIds.TryGetValue(processId, out Socket clientSocket);
                try
                {
                    if (clientSocket != null && clientSocket.Connected)
                    {
                        clientSocket.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(instanceCommand)));
                        return true;
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"{e.Message} {e.ErrorCode} {e.NativeErrorCode} {e.SocketErrorCode}");
                }
            }
            return false;
        }
    }
}
