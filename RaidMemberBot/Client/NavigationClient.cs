using Newtonsoft.Json;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace RaidMemberBot.Client
{
    public class NavigationClient
    {
        private Socket _pathfindingSocket;

        readonly int BufferSize = 1024;
        public bool isRaidLeader;

        public static NavigationClient Instance { get; private set; } = new NavigationClient();

        private NavigationClient()
        {
        }

        public Position[] CalculatePath(uint mapId, Position startPosition, Position endPosition, bool smoothPath)
        {
            PathfindingRequest request = new PathfindingRequest()
            {
                IsRaidLeader = isRaidLeader,
                MapId = mapId,
                StartPosition = new Vector3(startPosition.X, startPosition.Y, startPosition.Z),
                EndPosition = new Vector3(endPosition.X, endPosition.Y, endPosition.Z),
                SmoothPath = smoothPath
            };

            string json = SendRequest(request);

            Vector3[] path = JsonConvert.DeserializeObject<Vector3[]>(json);
            return path.Select(x => new Position(x.X, x.Y, x.Z)).ToArray();
        }
        public float CalculatePathingDistance(uint mapId, Position startPosition, Position endPosition, bool smoothPath)
        {
            Position[] locations = CalculatePath(mapId, startPosition, endPosition, smoothPath);
            return CalculatePathingDistance(locations);
        }
        public float CalculatePathingDistance(Position[] locations)
        {
            float distance = 0;
            for (int i = 0; i < locations.Length - 1; i++)
            {
                distance += locations[i].DistanceTo(locations[i + 1]);
            }

            return distance;
        }

        private string SendRequest(PathfindingRequest pathfindingRequest)
        {
            try
            {
                _pathfindingSocket ??= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (!_pathfindingSocket.Connected)
                {
                    try
                    {
                        _pathfindingSocket.Connect(IPAddress.Parse(RaidMemberSettings.Instance.ListenAddress), ConfigClient.Instance.ConfigurationResponse.NavigationServerPort);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"NAVIGATION CLIENT: {e.StackTrace}");
                    }
                }
                if (_pathfindingSocket.Connected)
                {
                    string databaseRequestJson = JsonConvert.SerializeObject(pathfindingRequest);

                    _pathfindingSocket.Send(Encoding.ASCII.GetBytes(databaseRequestJson));

                    byte[] buffer = ReceiveMessage();

                    return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"NAVIGATION CLIENT: {e.StackTrace}");
                try
                {
                    _pathfindingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                catch
                {
                    _pathfindingSocket.Close();
                }
            }

            return string.Empty;
        }

        public byte[] ReceiveMessage()
        {
            byte[] messageBuffer = new byte[BufferSize];
            int totalBytesReceived = 0;

            int bytesReceived;

            int arrayBeginTokens = 0;
            int arrayEndTokens = 0;

            int objectBeginTokens = 0;
            int objectEndTokens = 0;
            do
            {
                byte[] buffer = new byte[BufferSize];

                // Receive at most the requested number of bytes, or the amount the 
                // buffer can hold, whichever is smaller.
                bytesReceived = _pathfindingSocket.Receive(buffer);

                if (messageBuffer.Length < totalBytesReceived + bytesReceived)
                {
                    Array.Resize(ref messageBuffer, messageBuffer.Length * 2);
                }

                // Copy the receive buffer into the message buffer, appending after 
                // previously received data (totalBytesReceived).
                Buffer.BlockCopy(buffer, 0, messageBuffer, totalBytesReceived, bytesReceived);

                totalBytesReceived += bytesReceived;
                string s = Encoding.UTF8.GetString(buffer);

                arrayBeginTokens += s.Count(x => x == '[');
                arrayEndTokens += s.Count(x => x == ']');

                objectBeginTokens += s.Count(x => x == '{');
                objectEndTokens += s.Count(x => x == '}');

                if (arrayBeginTokens == arrayEndTokens && objectBeginTokens == objectEndTokens)
                {
                    break;
                }
            } while (bytesReceived == BufferSize);

            Array.Resize(ref messageBuffer, totalBytesReceived);

            return messageBuffer;
        }
    }
}
