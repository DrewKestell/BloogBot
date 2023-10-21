using Newtonsoft.Json;
using RaidMemberBot;
using RaidMemberBot.Models.Dto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaidLeaderBot
{
    public class DatabaseSocketServer : IDisposable
    {
        private readonly object _lockObject = new object();
        private readonly int _port;
        private readonly IPAddress _ipAddress;
        private readonly Dictionary<int, Socket> _processIds = new Dictionary<int, Socket>();
        private Socket _listener;
        private Task _backgroundTask;
        private bool _listen;

        public DatabaseSocketServer(int port, IPAddress ipAddress)
        {
            _port = port;
            _ipAddress = ipAddress;
        }

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

                    lock (_lockObject)
                    {
                        if (receivedDataLength == 0)
                        {
                            continue;
                        }
                        json = Encoding.UTF8.GetString(buffer, 0, receivedDataLength);

                        DatabaseRequest request = JsonConvert.DeserializeObject<DatabaseRequest>(json);
                        string response = "{}";

                        switch (request.QueryType)
                        {
                            case QueryType.GetCreatureMovementByGuid:
                                response = JsonConvert.SerializeObject(SqliteRepository.GetCreatureMovementById(int.Parse(request.QueryParam1)));
                                break;
                            case QueryType.GetCreatureLinkedByGuid:
                                response = JsonConvert.SerializeObject(SqliteRepository.GetCreatureLinkedByGuid(int.Parse(request.QueryParam1)));
                                break;
                            case QueryType.GetCreatureTemplateById:
                                response = JsonConvert.SerializeObject(SqliteRepository.GetCreatureTemplateById(int.Parse(request.QueryParam1)));
                                break;
                            case QueryType.GetCreaturesById:
                                response = JsonConvert.SerializeObject(SqliteRepository.GetCreaturesById(int.Parse(request.QueryParam1)));
                                break;
                            case QueryType.GetCreaturesByMapId:
                                response = JsonConvert.SerializeObject(SqliteRepository.GetCreaturesByMapId(int.Parse(request.QueryParam1)));
                                break;
                            case QueryType.GetItemById:
                                response = JsonConvert.SerializeObject(SqliteRepository.GetItemById(ulong.Parse(request.QueryParam1)));
                                break;
                            case QueryType.GetDungeonStartingPoint:
                                response = JsonConvert.SerializeObject(SqliteRepository.GetAreaTriggerTeleportByMapId(int.Parse(request.QueryParam1)));
                                break;
                            case QueryType.GetCreatureEquipTemplateById:
                                response = JsonConvert.SerializeObject(SqliteRepository.GetCreatureEquipTemplateById(int.Parse(request.QueryParam1)));
                                break;
                        }
                        byte[] bytes = Encoding.ASCII.GetBytes(response);
                        while (bytes.Length > 1024)
                        {
                            byte[] tempBytes = new byte[1024];
                            Array.Copy(bytes, tempBytes, 1024);

                            clientSocket.Send(tempBytes);

                            Array.Reverse(bytes);
                            Array.Resize(ref bytes, bytes.Length - 1024);
                            Array.Reverse(bytes);
                        }
                        if (bytes.Length > 0)
                        {
                            clientSocket.Send(bytes);
                        }
                    }
                    Array.Clear(buffer, 0, buffer.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Process {processId} disconnected due to {e.StackTrace}");
                    _processIds.Remove(processId);
                    clientSocket.Close();
                    return;
                }
            }
            _processIds.Remove(processId);
            clientSocket.Close();
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
        }
    }

}
