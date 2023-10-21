using Newtonsoft.Json;
using RaidLeaderBot.Objects;
using RaidLeaderBot.Pathfinding;
using RaidMemberBot.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaidLeaderBot
{
    public class NavigationSocketServer : IDisposable
    {
        private readonly object _lockObject = new object();
        private readonly int _port;
        private readonly IPAddress _ipAddress;
        private readonly Dictionary<int, Socket> _processIds = new Dictionary<int, Socket>();
        private Socket _listener;
        private Task _backgroundTask;
        private bool _listen;
        private Location _lastKnownPlayerLoc = new Location();

        public NavigationSocketServer(int port, IPAddress ipAddress)
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

                        PathfindingRequest request = JsonConvert.DeserializeObject<PathfindingRequest>(json);

                        Location startLocation = new Location(request.StartLocation.X, request.StartLocation.Y, request.StartLocation.Z);
                        Location endLocation = new Location(request.EndLocation.X, request.EndLocation.Y, request.EndLocation.Z);

                        if (endLocation.X == 0 && endLocation.Y == 0 && endLocation.Z == 0) {
                            endLocation = _lastKnownPlayerLoc;
                        }

                        Location[] path = Navigation.Instance.CalculatePath(request.MapId, startLocation, endLocation, request.SmoothPath);

                        string response = JsonConvert.SerializeObject(path.Select(x => new Vector3(x.X, x.Y, x.Z)));

                        byte[] bytes = Encoding.ASCII.GetBytes(response);
                        int totalSent = 0;

                        while (bytes.Length > 1024)
                        {
                            byte[] tempBytes = new byte[1024];
                            Array.Copy(bytes, tempBytes, 1024);

                            totalSent += clientSocket.Send(tempBytes);

                            Array.Reverse(bytes);
                            Array.Resize(ref bytes, bytes.Length - 1024);
                            Array.Reverse(bytes);
                        }
                        if (bytes.Length > 0)
                        {
                            totalSent += clientSocket.Send(bytes);
                        }

                        if (request.IsRaidLeader)
                        {
                            _lastKnownPlayerLoc = startLocation;
                        }
                    }
                    Array.Clear(buffer, 0, buffer.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Process {processId} disconnected due to {e.GetType()}");
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
