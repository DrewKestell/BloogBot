using BaseSocketServer;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using WoWActivityMember.Models;

namespace WoWStateManager.Listeners
{
    public class WorldStateActivitySocketListener() : AbstractSocketServer(8089, IPAddress.Loopback)
    {
        private readonly Subject<ActivityState> _instanceUpdateSubject = new();

        public IObservable<ActivityState> InstanceUpdateObservable => _instanceUpdateSubject;

        public override Guid HandleRequest(byte[] payload, Socket clientSocket)
        {
            if (payload.Length == 0)
            {
                // TODO: Log error
                return Guid.Empty;
            }

            var activityState = JsonConvert.DeserializeObject<ActivityState>(Encoding.UTF8.GetString(payload));

            if (activityState.ServiceId != Guid.Empty)
            {
                if (serviceIds.TryAdd(activityState.ServiceId, clientSocket))
                {
                    Console.WriteLine($"{DateTime.Now}|[WorldStateActivitySocketListener:{_port}]:HandleRequest WoWActivityManager connected {activityState.ServiceId}");
                }

                _instanceUpdateSubject.OnNext(activityState);
            }
            return activityState.ServiceId;
        }

        public bool SendCommandToProcess(Guid serviceId, ActivityState activityState)
        {
            if (serviceIds.ContainsKey(serviceId))
            {
                serviceIds.TryGetValue(serviceId, out Socket clientSocket);
                try
                {
                    if (clientSocket != null && clientSocket.Connected)
                    {
                        clientSocket.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(activityState)));
                        return true;
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"{DateTime.Now}|[WorldStateActivitySocketListener:{_port}]:SendCommandToProcess {e.Message} {e.ErrorCode} {e.NativeErrorCode} {e.SocketErrorCode}");
                }
            }
            return false;
        }
    }
}
