using GameData.Core.Enums;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using WowSrp.Header;

namespace WoWSharpClient.Client
{
    internal class WorldClient(OpCodeDispatcher opCodeDispatcher) : IDisposable
    {
        private readonly OpCodeDispatcher _opCodeDispatcher = opCodeDispatcher;

        //TODO: Take in from constructor
        private IPAddress _ipAddress = IPAddress.Loopback;
        private int _port = 0;

        private TcpClient? _client = null;
        private VanillaDecryption _vanillaDecryption;
        private VanillaEncryption _vanillaEncryption;
        private NetworkStream _stream = null;
        private Task _asyncListener;
        private readonly uint _lastPingTime;

        private readonly ConcurrentQueue<Func<Task>> _sendQueue = new();
        private readonly SemaphoreSlim _sendSemaphore = new(1, 1);
        private bool _isSending = false;

        public void Dispose()
        {
            _client?.Close();
        }

        public bool IsConnected => _client != null && _client.Connected;

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

        private void SendPacket(byte[] packetData, Opcode opcode)
        {
            EnqueueSend(async () =>
            {
                Console.WriteLine($"[WorldClient] Sending packet: {opcode} ({packetData.Length} bytes)");
                await _stream.WriteAsync(packetData);
            });
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

            ushort size = BitConverter.ToUInt16([.. header.Take(2).Reverse()], 0);
            ushort opcode = BitConverter.ToUInt16([.. header.Skip(2).Take(2)], 0);

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

                uint opcode = (uint)Opcode.CMSG_AUTH_SESSION;
                uint build = 5875;
                uint serverId = 1;
                uint clientSeed = (uint)new Random().Next(int.MinValue, int.MaxValue);

                byte[] clientProof = PacketManager.GenerateClientProof(username, clientSeed, serverSeed, sessionKey);
                byte[] decompressedAddonInfo = PacketManager.GenerateAddonInfo();
                byte[] compressedAddonInfo = PacketManager.Compress(decompressedAddonInfo);
                uint decompressedAddonInfoSize = (uint)decompressedAddonInfo.Length;

                ushort packetSize = (ushort)(4 + 4 + 4 + username.Length + 1 + 4 + clientProof.Length + 4 + compressedAddonInfo.Length);

                writer.Write(BitConverter.GetBytes(packetSize).Reverse().ToArray());
                writer.Write(opcode);
                writer.Write(build);
                writer.Write(serverId);
                writer.Write(Encoding.UTF8.GetBytes(username));
                writer.Write((byte)0);
                writer.Write(clientSeed);
                writer.Write(clientProof);
                writer.Write(decompressedAddonInfoSize);
                writer.Write(compressedAddonInfo);

                writer.Flush();
                byte[] packetData = memoryStream.ToArray();
                SendPacket(packetData, Opcode.CMSG_AUTH_SESSION);

                _asyncListener = Task.Run(HandleNetworkMessagesAsync);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WorldClient] An error occurred while sending CMSG_AUTH_SESSION: {ex}");
            }
        }

        public void SendCMSGCharEnum()
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            var header = _vanillaEncryption.CreateClientHeader(4, (uint)Opcode.CMSG_CHAR_ENUM);
            writer.Write(header);
            writer.Flush();
            SendPacket(ms.ToArray(), Opcode.CMSG_CHAR_ENUM);
        }

        public void SendCMSGQueryTime()
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            var header = _vanillaEncryption.CreateClientHeader(4, (uint)Opcode.CMSG_QUERY_TIME);
            writer.Write(header);
            writer.Flush();
            SendPacket(ms.ToArray(), Opcode.CMSG_QUERY_TIME);
        }

        public void SendCMSGPing(uint sequence)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            var header = _vanillaEncryption.CreateClientHeader(12, (uint)Opcode.CMSG_PING);
            writer.Write(header);
            writer.Write(sequence);
            writer.Write((uint)0);
            writer.Flush();
            SendPacket(ms.ToArray(), Opcode.CMSG_PING);
        }

        public void SendCMSGPlayerLogin(ulong guid)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            var header = _vanillaEncryption.CreateClientHeader(12, (uint)Opcode.CMSG_PLAYER_LOGIN);
            writer.Write(header);
            writer.Write(guid);
            writer.Flush();
            SendPacket(ms.ToArray(), Opcode.CMSG_PLAYER_LOGIN);
        }

        public void SendCMSGNameTypeQuery(Opcode opcode, ulong guid)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            var header = _vanillaEncryption.CreateClientHeader(12, (uint)opcode);
            writer.Write(header);
            writer.Write(guid);
            writer.Flush();
            SendPacket(ms.ToArray(), opcode);
        }

        public void SendCMSGMessageChat(ChatMsg type, Language language, string destinationName, string message)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms, Encoding.UTF8, true);
            byte[] header = _vanillaEncryption.CreateClientHeader(12 + ((uint)message.Length + 1) + (uint)(type == ChatMsg.CHAT_MSG_WHISPER || type == ChatMsg.CHAT_MSG_CHANNEL ? destinationName.Length + 1 : 0), (uint)Opcode.CMSG_MESSAGECHAT);
            writer.Write(header);
            writer.Write((uint)type);
            writer.Write((uint)language);

            if (type == ChatMsg.CHAT_MSG_WHISPER || type == ChatMsg.CHAT_MSG_CHANNEL)
            {
                writer.Write(Encoding.UTF8.GetBytes(destinationName));
                writer.Write((byte)0);
            }

            writer.Write(Encoding.UTF8.GetBytes(message));
            writer.Write((byte)0);

            writer.Flush();
            SendPacket(ms.ToArray(), Opcode.CMSG_MESSAGECHAT);
        }

        public void SendMSGMoveWorldportAck()
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            var header = _vanillaEncryption.CreateClientHeader(4, (uint)Opcode.MSG_MOVE_WORLDPORT_ACK);
            writer.Write(header);
            writer.Flush();
            SendPacket(ms.ToArray(), Opcode.MSG_MOVE_WORLDPORT_ACK);
        }

        public void SendCMSGSetActiveMover(ulong guid)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            var header = _vanillaEncryption.CreateClientHeader(12, (uint)Opcode.CMSG_SET_ACTIVE_MOVER);
            writer.Write(header);
            writer.Write(guid);
            writer.Flush();
            SendPacket(ms.ToArray(), Opcode.CMSG_SET_ACTIVE_MOVER);
        }

        public void SendMSGMove(Opcode opcode, byte[] movementInfo)
        {
            //Console.WriteLine($"[SendMSGMove] {opcode} {BitConverter.ToString(movementInfo)}");
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            var header = _vanillaEncryption.CreateClientHeader((uint)(4 + movementInfo.Length), (uint)opcode);
            writer.Write(header);
            writer.Write(movementInfo);
            writer.Flush();
            SendPacket(ms.ToArray(), opcode);
        }

        public void SendMSGPacked(Opcode opcode, byte[] payload)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            var header = _vanillaEncryption.CreateClientHeader((uint)(4 + payload.Length), (uint)opcode);
            writer.Write(header);
            writer.Write(payload);
            writer.Flush();
            SendPacket(ms.ToArray(), opcode);
        }

        public void SendCMSGCreateCharacter(string name, Race race, Class clazz, Gender gender, byte skin, byte face, byte hairStyle, byte hairColor, byte facialHair, byte outfitId)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms, Encoding.UTF8, true);
            byte[] header = _vanillaEncryption.CreateClientHeader((uint)(4 + name.Length + 10), (uint)Opcode.CMSG_CHAR_CREATE);
            writer.Write(header);
            writer.Write(Encoding.UTF8.GetBytes(name + "\0"));
            writer.Write((byte)race);
            writer.Write((byte)clazz);
            writer.Write((byte)gender);
            writer.Write(skin);
            writer.Write(face);
            writer.Write(hairStyle);
            writer.Write(hairColor);
            writer.Write(facialHair);
            writer.Write(outfitId);
            writer.Flush();
            SendPacket(ms.ToArray(), Opcode.CMSG_CHAR_CREATE);
        }

        private async Task HandleNetworkMessagesAsync()
        {
            WoWSharpEventEmitter.Instance.FireOnWorldSessionStart();
            using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
            byte[] body = [];
            while (true)
            {
                try
                {
                    byte[] header = PacketManager.Read(reader, 4);
                    if (header.Length == 0)
                    {
                        WoWSharpEventEmitter.Instance.FireOnWorldSessionEnd();
                        break;
                    }

                    HeaderData headerData = _vanillaDecryption.ReadServerHeader(header);
                    body = PacketManager.Read(reader, (int)(headerData.Size - sizeof(ushort)));
                    _opCodeDispatcher.Dispatch((Opcode)headerData.Opcode, body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WorldClient][HandleNetworkMessages] An error occurred while handling network messages: {ex} {BitConverter.ToString(body)}");
                }
            }
            await Task.Delay(10);
        }
        private void EnqueueSend(Func<Task> sendAction)
        {
            _sendQueue.Enqueue(sendAction);
            if (!_isSending)
            {
                _isSending = true;
                _ = Task.Run(ProcessSendQueueAsync);
            }
        }

        private async Task ProcessSendQueueAsync()
        {
            while (_sendQueue.TryDequeue(out var action))
            {
                try
                {
                    await _sendSemaphore.WaitAsync();
                    await action();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[WorldClient] Error during send: " + ex);
                }
                finally
                {
                    _sendSemaphore.Release();
                }
            }
            _isSending = false;
        }
    }
}
