using BaseSocketServer;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using WoWActivityMember.Models;

namespace WoWStateManager.Listeners
{
    public class WorldStateManagerSocketListener() : AbstractSocketServer(8089, IPAddress.Parse("127.0.0.1"))
    {
        private readonly Subject<WorldStateUpdate> _instanceUpdateSubject = new();

        public IObservable<WorldStateUpdate> InstanceUpdateObservable => _instanceUpdateSubject;

        public override int HandleRequest(string payload, Socket clientSocket)
        {
            if (string.IsNullOrEmpty(payload))
                return 0;

            WorldStateUpdate instanceUpdate = JsonConvert.DeserializeObject<WorldStateUpdate>(payload);
            int processId = instanceUpdate.ProcessId;

            if (processId != 0)
            {
                if (!_processIds.ContainsKey(processId))
                {
                    Console.WriteLine($"{DateTime.Now}|[WORLD STATE MANAGER SERVER : {_port}] Process connected {processId}");
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
                    Console.WriteLine($"{DateTime.Now}|[WORLD STATE MANAGER SERVER : {_port}] {e.Message} {e.ErrorCode} {e.NativeErrorCode} {e.SocketErrorCode}");
                }
            }
            return false;
        }
    }
}
