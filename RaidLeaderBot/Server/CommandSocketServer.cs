using Newtonsoft.Json;
using RaidMemberBot.Models.Dto;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;

namespace RaidLeaderBot
{
    public class CommandSocketServer : BaseSocketServer
    {
        private readonly Subject<CharacterState> _instanceUpdateSubject;

        public CommandSocketServer(int port, IPAddress ipAddress) : base(port, ipAddress)
        {
            _instanceUpdateSubject = new Subject<CharacterState>();
        }

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
                    Console.WriteLine($"[COMMAND SERVER {_port}]Process connected {processId}");
                    _processIds.Add(processId, clientSocket);
                }

                instanceUpdate.IsConnected = true;
                _instanceUpdateSubject.OnNext(instanceUpdate);
            }
            return processId;
        }

        public bool SendCommandToProcess(int processId, InstanceCommand instanceCommand)
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
