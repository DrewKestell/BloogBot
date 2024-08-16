using BotRunner.Constants;
using System.Net;
using System.Net.Sockets;
using System.Text;
using WoWSharpClient.Manager;
using WowSrp.Header;

namespace WoWSharpClient.Client
{
    internal class WorldClient(WoWSharpEventEmitter woWSharpEventEmitter, ObjectManager objectManager) : IDisposable
    {
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter = woWSharpEventEmitter;
        private readonly ObjectManager _objectManager = objectManager;
        private readonly OpCodeDispatcher _opCodeDispatcher = new (woWSharpEventEmitter, objectManager);
        private readonly bool _hasReceivedCharListReply;

        private IPAddress _ipAddress = IPAddress.Loopback;
        private int _port = 0;

        private TcpClient? _client = null;
        private VanillaDecryption _vanillaDecryption;
        private VanillaEncryption _vanillaEncryption;
        private NetworkStream _stream = null;
        private Task _asyncListener;
        private Task _serverPinger;
        private uint _lastPingTime;

        public void Dispose()
        {
            _client?.Close();
        }

        public void Connect(string username, IPAddress ipAddress, byte[] sessionKey, int port = 8085)
        {
            try
            {
                _ipAddress = ipAddress;
                _port = port;

                _vanillaDecryption = new VanillaDecryption(sessionKey);
                _vanillaEncryption = new VanillaEncryption(sessionKey);

                _client?.Close();
                _client = new TcpClient(AddressFamily.InterNetwork);
                _client.Connect(_ipAddress, _port);

                _stream = _client.GetStream();

                HandleAuthChallenge(username, sessionKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void StartServerPing()
        {
            _serverPinger = Task.Run(SendCMSGPing);
        }

        public void HandleAuthChallenge(string username, byte[] sessionKey)
        {
            using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
            byte[] header = reader.ReadBytes(4);

            if (header.Length < 4)
            {
                Console.WriteLine($"[WorldClient] Received incomplete SMSG_AUTH_CHALLENGE header.");
                return;
            }

            ushort size = BitConverter.ToUInt16(header.Take(2).Reverse().ToArray(), 0);
            ushort opcode = BitConverter.ToUInt16(header.Skip(2).Take(2).ToArray(), 0);

            byte[] serverSeed = reader.ReadBytes(size - sizeof(ushort));
            if (serverSeed.Length < 4)
            {
                Console.WriteLine($"[WorldClient] Incomplete SMSG_AUTH_CHALLENGE packet.");
                return;
            }

            SendCMSGAuthSession(username, serverSeed, sessionKey);
        }

        private void SendCMSGAuthSession(string username, byte[] serverSeed, byte[] sessionKey)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true);

                uint opcode = (uint)Opcodes.CMSG_AUTH_SESSION; // Opcode for CMSG_AUTH_SESSION
                uint build = 5875; // Revision of the client
                uint serverId = 1; // Server ID, this value may vary
                uint clientSeed = (uint)new Random().Next(int.MinValue, int.MaxValue); // Generate a random client seed

                byte[] clientProof = PacketManager.GenerateClientProof(username, clientSeed, serverSeed, sessionKey); // Generate the client proof

                byte[] decompressedAddonInfo = PacketManager.GenerateAddonInfo(); // Generate addon info

                byte[] compressedAddonInfo = PacketManager.Compress(decompressedAddonInfo); // Compress the addon info

                uint decompressedAddonInfoSize = (uint)decompressedAddonInfo.Length;

                ushort packetSize = (ushort)(4 + 4 + 4 + username.Length + 1 + 4 + clientProof.Length + 4 + compressedAddonInfo.Length);

                writer.Write(BitConverter.GetBytes(packetSize).Reverse().ToArray()); // Packet size (big-endian)
                writer.Write(opcode); // Opcode (little-endian)
                writer.Write(build); // Client build
                writer.Write(serverId); // Server ID
                writer.Write(Encoding.UTF8.GetBytes(username)); // Username
                writer.Write((byte)0); // Null terminator for username
                writer.Write(clientSeed); // Client seed
                writer.Write(clientProof); // Client proof
                writer.Write(decompressedAddonInfoSize); // Decompressed addon info size
                writer.Write(compressedAddonInfo);

                writer.Flush();
                byte[] packetData = memoryStream.ToArray();
                _stream.Write(packetData, 0, packetData.Length);

                _asyncListener = Task.Run(HandleNetworkMessagesAsync);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorldClient] An error occurred while sending CMSG_AUTH_SESSION: {ex}");
            }
        }

        public void SendCMSGCharEnum()
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true);
                byte[] header = _vanillaEncryption.CreateClientHeader(4, (uint)Opcodes.CMSG_CHAR_ENUM);
                writer.Write(header); // Packet size: 4 (big-endian)

                writer.Flush();
                byte[] packetData = memoryStream.ToArray();

                _stream.Write(packetData, 0, packetData.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while sending CMSG_CHAR_ENUM: " + ex.Message);
            }
        }
        private async Task SendCMSGPing()
        {
            uint sequenceId = 1;
            while (true)
            {
                _lastPingTime = (uint)Environment.TickCount;
                try
                {
                    using var memoryStream = new MemoryStream();
                    using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true);
                    byte[] header = _vanillaEncryption.CreateClientHeader(96, (uint)Opcodes.CMSG_PING);
                    writer.Write(header); // Opcode: CMSG_PING
                    writer.Write(sequenceId);
                    writer.Write(_lastPingTime); // Last ping time

                    writer.Flush();
                    byte[] packetData = memoryStream.ToArray();
                    _stream.Write(packetData, 0, packetData.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[WorldClient] An error occurred while sending CMSG_PING: " + ex.Message);
                }
                sequenceId++;
                await Task.Delay(30000);
            }
        }
        public void SendCMSGPlayerLogin(ulong characterGuid)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true);
                byte[] header = _vanillaEncryption.CreateClientHeader(12, (uint)Opcodes.CMSG_PLAYER_LOGIN);
                writer.Write(header); // Opcode: CMSG_PLAYER_LOGIN
                writer.Write(characterGuid); // Character GUID

                writer.Flush();
                byte[] packetData = memoryStream.ToArray();
                _stream.Write(packetData, 0, packetData.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorldClient] An error occurred while sending CMSG_PLAYER_LOGIN: {ex}");
            }
        }
        public void SendCMSGMessageChat(ChatMsg type, Language language, string destinationName, string message)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true);
                byte[] header = _vanillaEncryption.CreateClientHeader(12 + ((uint)message.Length + 1) + ((uint)(type == ChatMsg.CHAT_MSG_WHISPER || type == ChatMsg.CHAT_MSG_CHANNEL ? destinationName.Length + 1 : 0)), (uint)Opcodes.CMSG_MESSAGECHAT);
                writer.Write(header); // Opcode: CMSG_MESSAGECHAT
                writer.Write((uint)type); // Chat message type
                writer.Write((uint)language); // Language

                if (type == ChatMsg.CHAT_MSG_WHISPER || type == ChatMsg.CHAT_MSG_CHANNEL)
                {
                    writer.Write(Encoding.UTF8.GetBytes(destinationName));
                    writer.Write((byte)0); // Null terminator
                }

                writer.Write(Encoding.UTF8.GetBytes(message)); // Message
                writer.Write((byte)0); // Null terminator

                writer.Flush();
                byte[] packetData = memoryStream.ToArray();
                _stream.Write(packetData, 0, packetData.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorldClient] An error occurred while sending CMSG_MESSAGECHAT: {ex}");
            }
        }

        public void SendCMSGTypeQuery(Opcodes opcode, ulong guid)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true);
                byte[] header = _vanillaEncryption.CreateClientHeader(12, (uint)opcode);
                writer.Write(header);
                writer.Write(guid);

                writer.Flush();
                byte[] packetData = memoryStream.ToArray();
                _stream.Write(packetData, 0, packetData.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorldClient] An error occurred while sending CMSG_MESSAGECHAT: {ex}");
            }
        }
        private async Task HandleNetworkMessagesAsync()
        {
            _woWSharpEventEmitter.FireOnWorldSessionStart();
            using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
            byte[] body = [];
            while (true) // Loop to continuously read messages
            {
                try
                {
                    // Read the header first to determine the size and opcode
                    byte[] header = PacketManager.Read(reader, 4); // Adjust size if header structure is different
                    if (header.Length == 0)
                    {

                        _woWSharpEventEmitter.FireOnWorldSessionEnd();
                        break; // Exit if we cannot read a full header
                    }

                    HeaderData headerData = _vanillaDecryption.ReadServerHeader(header);

                    // Read the packet body
                    body = PacketManager.Read(reader, (int)(headerData.Size - sizeof(ushort))); // Adjust based on actual header size

                    // Dispatch the packet to the appropriate handler
                    _opCodeDispatcher.Dispatch((Opcodes)headerData.Opcode, body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WorldClient][HandleNetworkMessages] An error occurred while handling network messages: {ex} {BitConverter.ToString(body)}");
                }
            }
            await Task.Delay(10);
        }
    }
}
