using BaseSocketServer;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using WoWClientBot.Models;
using WoWStateManager.Models;

namespace WoWStateManager
{
    public class WorldStateManagerServer(int port, IPAddress ipAddress) : AbstractSocketServer(port, ipAddress)
    {
        private readonly Subject<ActivityCommand> _instanceUpdateSubject = new();

        public IObservable<ActivityCommand> InstanceUpdateObservable => _instanceUpdateSubject;

        public override int HandleRequest(string payload, Socket clientSocket)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return 0;
            }
            ActivityCommand instanceUpdate = JsonConvert.DeserializeObject<ActivityCommand>(payload);
            int processId = instanceUpdate.ProcessId;

            if (processId != 0)
            {
                if (!_processIds.ContainsKey(processId))
                {
                    Console.WriteLine($"{DateTime.Now}| [ACTIVITY MANAGER SERVER : {_port}]Process connected {processId}");
                    _processIds.Add(processId, clientSocket);
                }

                _instanceUpdateSubject.OnNext(instanceUpdate);
            }
            return processId;
        }

        public bool SendCommandToProcess(int processId, List<ActivityState> activityStates)
        {
            if (_processIds.ContainsKey(processId))
            {
                _processIds.TryGetValue(processId, out Socket clientSocket);
                try
                {
                    if (clientSocket != null && clientSocket.Connected)
                    {
                        string payload = JsonConvert.SerializeObject(activityStates);
                        clientSocket.Send(Encoding.ASCII.GetBytes(payload));
                        return true;
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"{DateTime.Now}| {e.Message} {e.ErrorCode} {e.NativeErrorCode} {e.SocketErrorCode}");
                }
            }
            return false;
        }
    }
}
