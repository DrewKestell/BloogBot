using Newtonsoft.Json;
using RaidMemberBot.Models.Dto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaidLeaderBot
{
    public class SocketServer : IDisposable
    {
        private readonly int _port;
        private readonly IPAddress _ipAddress;
        private readonly Subject<CharacterState> _instanceUpdateSubject;
        private readonly Dictionary<int, Socket> _processIds = new Dictionary<int, Socket>();
        private Socket _listener;
        private Task _backgroundTask;
        private bool _listen;

        public SocketServer(int port, IPAddress ipAddress)
        {
            this._port = port;
            this._ipAddress = ipAddress;
            this._instanceUpdateSubject = new Subject<CharacterState>();
        }

        public IObservable<CharacterState> InstanceUpdateObservable => this._instanceUpdateSubject;

        public void Start()
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _listener.Bind(new IPEndPoint(_ipAddress, _port));
            _listener.Listen(10);
            _listen = true;
            _processIds.Clear();
            _backgroundTask = Task.Run(StartAsync);
        }

        private async Task StartAsync()
        {
            while (_listen)
            {
                Socket clientSocket = _listener.Accept();
                ThreadPool.QueueUserWorkItem(_ => HandleClient(clientSocket));
                await Task.Delay(100);
            }
        }

        private void HandleClient(Socket clientSocket)
        {
            byte[] buffer = new byte[1024];
            string json;
            int processId = 0;
            while (_listen)
            {
                try
                {
                    int receivedDataLength = clientSocket.Receive(buffer);
                    if (receivedDataLength == 0)
                    {
                        break;
                    }

                    json = Encoding.UTF8.GetString(buffer, 0, receivedDataLength);
                    Console.WriteLine(json);
                    CharacterState instanceUpdate = JsonConvert.DeserializeObject<CharacterState>(json);
                    processId = instanceUpdate.ProcessId;                    

                    if (processId != 0)
                    {
                        if (!_processIds.ContainsKey(processId))
                        {
                            _processIds.Add(processId, clientSocket);
                        }

                        instanceUpdate.IsConnected = true;
                        _instanceUpdateSubject.OnNext(instanceUpdate);
                    }

                    Array.Clear(buffer, 0, buffer.Length);
                }
                catch (Exception e)
                {
                    if (e.GetType() == typeof(SocketException))
                    {
                        Console.WriteLine(string.Format("Process {0} disconnected due to {1}", processId, e.GetType().ToString()));

                        _instanceUpdateSubject.OnNext(new CharacterState() { ProcessId = processId, IsConnected = false });
                        break;
                    }
                }
            }
            _processIds.Remove(processId);
            clientSocket.Close();
        }

        public bool SendCommandToProcess(int processId, InstanceCommand instanceCommand)
        {
            if (_processIds.ContainsKey(processId))
            {
                _processIds.TryGetValue(processId, out var clientSocket);
                try
                {
                    if (clientSocket != null && clientSocket.Connected)
                    {
                        clientSocket.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(instanceCommand)));
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    try
                    {
                        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    }
                    catch
                    {
                        clientSocket.Close();
                    }
                }
            }
            return false;
        }

        public void Stop()
        {
            _listen = false;
            _backgroundTask.Wait(TimeSpan.FromSeconds(5));
            _listener?.Close();
            _listener = null;
        }

        public void Dispose()
        {
            Stop();
            _instanceUpdateSubject?.Dispose();
        }
    }

}
