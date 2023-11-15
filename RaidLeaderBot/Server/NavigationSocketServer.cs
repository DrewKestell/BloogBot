using Newtonsoft.Json;
using RaidLeaderBot.Pathfinding;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
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
        private Position _lastKnownPlayerLoc = new Position(0, 0, 0);

        public NavigationSocketServer(int port, IPAddress ipAddress) : base(port, ipAddress)
        {
            Console.WriteLine($"[NAVIGATION SERVER]Port {port}");
        }

        public override int HandleRequest(string payload, Socket clientSocket)
        {
            PathfindingRequest request = JsonConvert.DeserializeObject<PathfindingRequest>(payload);

            Position startPosition = new Position(request.StartPosition.X, request.StartPosition.Y, request.StartPosition.Z);
            Position endPosition = new Position(request.EndPosition.X, request.EndPosition.Y, request.EndPosition.Z);

            if (endPosition.X == 0 && endPosition.Y == 0 && endPosition.Z == 0)
            {
                endPosition = _lastKnownPlayerLoc;
            }

            Position[] path = Navigation.Instance.CalculatePath(request.MapId, startPosition, endPosition, request.SmoothPath);

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
                _lastKnownPlayerLoc = startPosition;
            }
            return 0;
        }
    }
}
