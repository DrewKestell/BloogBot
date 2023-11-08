using Newtonsoft.Json;
using RaidLeaderBot.Objects;
using RaidLeaderBot.Pathfinding;
using RaidMemberBot.Models.Dto;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace RaidLeaderBot
{
    public class NavigationSocketServer : BaseSocketServer
    {
        private Location _lastKnownPlayerLoc = new Location();

        public NavigationSocketServer(int port, IPAddress ipAddress) : base(port, ipAddress)
        {
            Console.WriteLine($"NAVIGATION SERVER:Port {port}");
        }

        public override int HandleRequest(string payload, Socket clientSocket)
        {
            PathfindingRequest request = JsonConvert.DeserializeObject<PathfindingRequest>(payload);

            Location startLocation = new Location(request.StartLocation.X, request.StartLocation.Y, request.StartLocation.Z);
            Location endLocation = new Location(request.EndLocation.X, request.EndLocation.Y, request.EndLocation.Z);

            if (endLocation.X == 0 && endLocation.Y == 0 && endLocation.Z == 0)
            {
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
            return 0;
        }
    }
}
