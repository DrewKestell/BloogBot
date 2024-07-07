using BaseSocketServer;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using WoWActivityMember.Models;

namespace WoWActivityManager.Listeners
{
    public class WoWActivityMemberListener(IPAddress address, int port) : AbstractSocketServer(port, address)
    {
        private readonly Subject<ActivityMemberState> _instanceUpdateSubject = new();

        public IObservable<ActivityMemberState> InstanceUpdateObservable => _instanceUpdateSubject;
        public override Guid HandleRequest(byte[] payload, Socket clientSocket)
        {
            if (payload.Length == 0)
            {
                // TODO: Log error
                return Guid.Empty;
            }
            var activityMemberState = JsonConvert.DeserializeObject<ActivityMemberState>(Encoding.UTF8.GetString(payload));

            if (activityMemberState.ServiceId != Guid.Empty)
            {
                if (serviceIds.TryAdd(activityMemberState.ServiceId, clientSocket))
                {
                    Console.WriteLine($"{DateTime.Now}|[WoWActivityMemberListener:{_port}]:HandleRequest Process connected {activityMemberState.ServiceId}");
                }

                _instanceUpdateSubject.OnNext(activityMemberState);
            }
            return activityMemberState.ServiceId;
        }

        public bool SendCommandToProcess(Guid serviceId, ActivityMemberState activityMemberState)
        {
            if (serviceIds.ContainsKey(serviceId))
            {
                serviceIds.TryGetValue(serviceId, out Socket clientSocket);
                try
                {
                    if (clientSocket != null && clientSocket.Connected)
                    {
                        clientSocket.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(activityMemberState)));
                        return true;
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"{DateTime.Now}|[WoWActivityMemberListener:{_port}]:SendCommandToProcess {e.Message} {e.ErrorCode} {e.NativeErrorCode} {e.SocketErrorCode}");
                }
            }
            return false;
        }
    }
}
