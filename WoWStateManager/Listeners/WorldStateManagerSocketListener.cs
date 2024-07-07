using BaseSocketServer;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using WoWActivityMember.Models;

namespace WoWStateManager.Listeners
{
    public class WorldStateManagerSocketListener() : AbstractSocketServer(8088, IPAddress.Loopback)
    {
        private readonly Subject<WorldStateUpdate> _instanceUpdateSubject = new();
        private readonly Dictionary<int, Socket> serviceIds = [];

        public IObservable<WorldStateUpdate> InstanceUpdateObservable => _instanceUpdateSubject;

        public override Guid HandleRequest(byte[] payload, Socket clientSocket)
        {
            if (payload.Length == 0)
            {
                // TODO: Log error
                return Guid.Empty;
            }

            WorldStateUpdate instanceUpdate = JsonConvert.DeserializeObject<WorldStateUpdate>(Encoding.ASCII.GetString(payload));

            int processId = instanceUpdate.ProcessId;

            if (processId != 0)
            {
                if (serviceIds.TryAdd(processId, clientSocket))
                {
                    Console.WriteLine($"{DateTime.Now}|[WorldStateManagerSocketListener:{_port}]:HandleRequest Process connected {processId}");
                }

                _instanceUpdateSubject.OnNext(instanceUpdate);
            }
            return Guid.Empty;
        }

        public bool SendCommandToProcess(int processId, List<ActivityState> activityStates)
        {
            if (serviceIds.ContainsKey(processId))
            {
                serviceIds.TryGetValue(processId, out Socket clientSocket);
                try
                {
                    if (clientSocket != null && clientSocket.Connected)
                    {
                        clientSocket.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(activityStates)));
                        return true;
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"{DateTime.Now}|[WorldStateManagerSocketListener:{_port}]:SendCommandToProcess {e.Message} {e.ErrorCode} {e.NativeErrorCode} {e.SocketErrorCode}");
                }
            }
            return false;
        }
    }
}
