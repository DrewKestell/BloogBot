using Newtonsoft.Json;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace RaidMemberBot.Client
{
    public class SocketClient
    {
        private Socket _socket;
        private Socket _pathfindingSocket;

        readonly RaidMemberSettings raidMemberSettings;

        bool isRaidLeader;

        public static SocketClient Instance { get; private set; } = new SocketClient();

        private SocketClient()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var botSettingsFilePath = Path.Combine(currentFolder, "Settings\\raidMemberSettings.json");
            raidMemberSettings = JsonConvert.DeserializeObject<RaidMemberSettings>(File.ReadAllText(botSettingsFilePath));
        }

        public InstanceCommand GetCommandBasedOnState(CharacterState characterState)
        {
            isRaidLeader = string.IsNullOrEmpty(characterState.RaidLeader) && characterState.RaidLeader == characterState.CharacterName;

            byte[] buffer = new byte[1024];

            try
            {
                _socket ??= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (!_socket.Connected)
                {
                    try
                    {
                        _socket.Connect(IPAddress.Parse(raidMemberSettings.ListenAddress), raidMemberSettings.Port);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                if (_socket.Connected)
                {
                    string instanceUpdateJson = JsonConvert.SerializeObject(characterState);

                    _socket.Send(Encoding.ASCII.GetBytes(instanceUpdateJson));

                    int receivedDataLength = _socket.Receive(buffer);

                    if (receivedDataLength > 0)
                    {
                        string json = Encoding.UTF8.GetString(buffer, 0, receivedDataLength);
                        if (json.Length > 0 && json.StartsWith("{"))
                        {
                            try
                            {
                                InstanceCommand instanceCommand = JsonConvert.DeserializeObject<InstanceCommand>(json);

                                if (instanceCommand != null)
                                {
                                    return instanceCommand;
                                }
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                try
                {
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                catch
                {
                    _socket.Close();
                }
            }

            return new InstanceCommand();
        }

        public Location[] CalculatePath(uint mapId, Location startLocation, Location endLocation, bool smoothPath)
        {
            byte[] buffer = new byte[1024];

            PathfindingRequest request = new PathfindingRequest()
            {
                IsRaidLeader = isRaidLeader,
                MapId = mapId,
                StartLocation = new Vector3(startLocation.X, startLocation.Y, startLocation.Z),
                EndLocation = new Vector3(endLocation.X, endLocation.Y, endLocation.Z),
                SmoothPath = smoothPath
            };

            try
            {
                _pathfindingSocket ??= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (!_pathfindingSocket.Connected)
                {
                    try
                    {
                        _pathfindingSocket.Connect(IPAddress.Parse(raidMemberSettings.ListenAddress), raidMemberSettings.PathfindingPort);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                if (_pathfindingSocket.Connected)
                {
                    string pathingRequestJson = JsonConvert.SerializeObject(request);

                    _pathfindingSocket.Send(Encoding.ASCII.GetBytes(pathingRequestJson));

                    int receivedDataLength = _pathfindingSocket.Receive(buffer);

                    if (receivedDataLength > 0)
                    {
                        string json = Encoding.UTF8.GetString(buffer, 0, receivedDataLength);

                        if (json.Length > 0 && json.StartsWith("["))
                        {
                            try
                            {
                                Vector3[] path = JsonConvert.DeserializeObject<Vector3[]>(json);

                                if (path != null)
                                {
                                    return path.Select(x => new Location(x.X, x.Y, x.Z)).ToArray();
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Waypoint error {e.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                try
                {
                    _pathfindingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                catch
                {
                    _pathfindingSocket.Close();
                }
            }
            return Array.Empty<Location>();
        }
    }
}
