using BaseSocketServer;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using Communication;
using WoWActivityMember.Models;

namespace WoWActivityManager.Listeners
{
    public class WoWActivityMemberListener(IPAddress address, int port) : AbstractSocketServer(port, address)
    {
        private readonly Subject<DataMessage> _instanceUpdateSubject = new();

        public IObservable<DataMessage> InstanceUpdateObservable => _instanceUpdateSubject;
        public override int HandleRequest(byte[] payload, Socket clientSocket)
        {
            if (payload.Length == 0)
            {
                // TODO: Log error
                return 0;
            }

            var dataMessage = DataMessage.Parser.ParseFrom(payload);
            //int processId = instanceUpdate.ProcessId;
            int processId = 0; //TODO: implement communication protocol new

            if (processId != 0)
            {
                if (!_processIds.ContainsKey(processId))
                {
                    Console.WriteLine($"{DateTime.Now}|[ACTIVITY MANAGER SERVER : {_port}]Process connected {processId}");
                    _processIds.Add(processId, clientSocket);
                }

                _instanceUpdateSubject.OnNext(dataMessage);
            }
            return processId;
        }

        public bool SendCommandToProcess(int processId, ActivityMemberState activityMemberState)
        {
            if (_processIds.ContainsKey(processId))
            {
                _processIds.TryGetValue(processId, out Socket clientSocket);
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
                    Console.WriteLine($"{DateTime.Now}|[ACTIVITY MANAGER SERVER {_port}]{e.Message} {e.ErrorCode} {e.NativeErrorCode} {e.SocketErrorCode}");
                }
            }
            return false;
        }
    }
}
