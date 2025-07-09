using GameData.Core.Enums;
using GameData.Core.Models;
using Org.BouncyCastle.Utilities;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using WowSrp.Client;

namespace WoWSharpClient.Client
{
    internal class AuthLoginClient(string ipAddress, WoWSharpEventEmitter woWSharpEventEmitter) : IDisposable
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private readonly IPAddress _ipAddress = IPAddress.Parse(ipAddress);
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter = woWSharpEventEmitter;

        private TcpClient? _client = null;
        private SrpClient _srpClient;
        private SrpClientChallenge _srpClientChallenge;
        private NetworkStream _stream = null;

        private byte[] _serverProof = [];

        public string Username => _username;
        public IPAddress IPAddress => _ipAddress;
        public byte[] ServerProof => _serverProof;
        public byte[] SessionKey => _srpClient.SessionKey;
        public bool IsConnected => _client != null && _client.Connected;

        public void Connect()
        {
            try
            {
                _client?.Close();
                _client = new TcpClient(AddressFamily.InterNetwork);
                _client.Connect(_ipAddress, 3724);
                _stream = _client.GetStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginClient] {ex}");
            }
        }

        public void Login(string username, string password)
        {
            _username = username;
            _password = password;

            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true);
            int packetSize = 30 + _username.Length;

            writer.Write((byte)0x00); // Opcode: CMD_AUTH_LOGON_CHALLENGE
            writer.Write((byte)0x03); // Protocol Version: 3
            writer.Write((ushort)packetSize); // Packet Size (little-endian)

            writer.Write(Encoding.UTF8.GetBytes("WoW\0")); // Game Name: "WoW\0"
            writer.Write((byte)0x01); // Major Version: 1
            writer.Write((byte)0x0C); // Minor Version: 12
            writer.Write((byte)0x01); // Patch Version: 1
            writer.Write((ushort)0x16F3); // Build: 5875 (little-endian)

            writer.Write(Encoding.UTF8.GetBytes("68x\0")); // Platform: "68x\0"
            writer.Write(Encoding.UTF8.GetBytes("niW\0")); // OS: "niW\0"
            writer.Write(Encoding.UTF8.GetBytes("BGne")); // Locale: "enGB"

            writer.Write((uint)60); // Timezone Bias
            writer.Write((uint)0x0100007F); // Client IP

            writer.Write((byte)_username.Length); // Username length
            writer.Write(Encoding.UTF8.GetBytes(_username.ToUpper()));

            writer.Flush();
            byte[] packetData = memoryStream.ToArray();

            Console.WriteLine($"[AuthLoginClient] -> CMD_AUTH_LOGON_CHALLENGE [{packetData.Length}]");

            _woWSharpEventEmitter.FireOnHandshakeBegin();
            _stream.Write(packetData, 0, packetData.Length);

            ReceiveAuthLogonChallengeServer();
        }

        private void ReceiveAuthLogonChallengeServer()
        {
            try
            {
                using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                byte[] packet = reader.ReadBytes(119); // Adjust length if necessary

                Console.WriteLine($"[AuthLoginClient] <- CMD_AUTH_LOGON_CHALLENGE [{packet.Length}]");

                byte opcode = packet[0];
                ResponseCode result = (ResponseCode)packet[2];

                if (opcode == 0x00 && result == ResponseCode.RESPONSE_SUCCESS)
                {
                    byte[] serverPublicKey = [.. packet.Skip(3).Take(32)];
                    byte generator = packet[36];
                    byte largeSafePrimeLength = packet[37];
                    byte[] largeSafePrime = [.. packet.Skip(38).Take(largeSafePrimeLength)];
                    byte[] salt = [.. packet.Skip(70).Take(32)];
                    byte[] crcSalt = [.. packet.Skip(102).Take(16)];

                    if (packet.Length > 118 && packet[118] == 0x01)
                    {
                        uint pinGridSeed = BitConverter.ToUInt32(packet, 119);
                        byte[] pinSalt = [.. packet.Skip(123).Take(16)];
                    }

                    SendLogonProof(serverPublicKey, generator, largeSafePrime, salt, crcSalt);
                }
                else
                {
                    Console.WriteLine($"[AuthLoginClient] Unexpected AUTH_CHALLENGE response: opcode {opcode:X2}, result {result}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthLoginClient] Error in ReceiveAuthLogonChallengeServer: {ex}");
            }
        }

        private void SendLogonProof(byte[] serverPublicKey, byte generator, byte[] largeSafePrime, byte[] salt, byte[] crcSalt)
        {
            try
            {
                _srpClientChallenge = new SrpClientChallenge(_username, _password, generator, largeSafePrime, serverPublicKey, salt);
                byte[] crcHash = SHA1.HashData(Arrays.Concatenate(crcSalt, _srpClientChallenge.ClientProof));

                using var memoryStream = new MemoryStream();
                using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true);
                writer.Write((byte)0x01); // Opcode: CMD_AUTH_LOGON_PROOF

                writer.Write(_srpClientChallenge.ClientPublicKey);
                writer.Write(_srpClientChallenge.ClientProof);
                writer.Write(crcHash);

                writer.Write((byte)0x00); // Num keys
                writer.Write((byte)0x00); // 2FA disabled

                writer.Flush();
                byte[] packetData = memoryStream.ToArray();

                Console.WriteLine($"[AuthLoginClient] -> CMD_AUTH_LOGON_PROOF [{packetData.Length}]");

                _stream.Write(packetData, 0, packetData.Length);
                ReceiveAuthProofLogonResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginClient] Error in SendLogonProof: {ex}");
            }
        }

        private void ReceiveAuthProofLogonResponse()
        {
            try
            {
                using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                byte[] header = reader.ReadBytes(2);

                Console.WriteLine($"[AuthLoginClient] <- CMD_AUTH_LOGON_PROOF [{header.Length}]");

                if (header.Length < 2)
                    return;

                byte opcode = header[0];
                ResponseCode result = (ResponseCode)header[1];

                if (opcode == 0x01 && result == ResponseCode.RESPONSE_SUCCESS)
                {
                    byte[] body = reader.ReadBytes(24);

                    byte[] serverProof = [.. body.Take(20)];
                    _serverProof = serverProof;

                    var verificationResult = _srpClientChallenge.VerifyServerProof(serverProof);
                    if (!verificationResult.HasValue)
                        _woWSharpEventEmitter.FireOnLoginFailure();
                    else
                    {
                        _srpClient = verificationResult.Value;
                        _woWSharpEventEmitter.FireOnLoginSuccess();
                    }
                }
                else
                {
                    Console.WriteLine($"[AuthLoginClient] Failed AUTH_PROOF response: opcode {opcode:X2}, result {result}");
                    _woWSharpEventEmitter.FireOnLoginFailure();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthLoginClient] Error in ReceiveAuthProofLogonResponse: {ex}");
                _woWSharpEventEmitter.FireOnLoginFailure();
            }
        }

        private void SendRealmListRequest()
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true);
                writer.Write((ushort)0x10); // Size
                writer.Write((ushort)0x10); // Opcode
                writer.Write((ushort)0x00); // Padding

                writer.Flush();
                byte[] packetData = memoryStream.ToArray();

                Console.WriteLine($"[AuthLoginClient] -> CMD_REALM_LIST ({packetData.Length} bytes)");

                _stream.Write(packetData, 0, packetData.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginClient] Error in SendRealmListRequest: {ex}");
            }
        }

        public List<Realm> GetRealmList()
        {
            List<Realm> realms = [];

            SendRealmListRequest();

            try
            {
                using var reader = new BinaryReader(_stream, Encoding.UTF8, true);

                // STEP 1: Read header (opcode + size)
                byte opcode = reader.ReadByte();
                ushort size = reader.ReadUInt16();

                Console.WriteLine($"[RealmList] Opcode: 0x{opcode:X2}, Size: {size}");

                if (opcode != 0x10)
                {
                    Console.WriteLine("[RealmList] Unexpected opcode received.");
                    return realms;
                }

                // STEP 2: Read rest of packet body into a buffer of exactly [size] bytes
                byte[] body = reader.ReadBytes(size);

                if (body.Length < size)
                {
                    Console.WriteLine($"[RealmList] Warning: expected {size} bytes, got {body.Length}. Incomplete packet.");
                    return realms;
                }

                // STEP 3: Parse body safely
                using var bodyStream = new MemoryStream(body);
                using var bodyReader = new BinaryReader(bodyStream, Encoding.UTF8, true);

                ushort unknown = bodyReader.ReadUInt16();   // Usually 0x0000
                byte realmCount = bodyReader.ReadByte();

                Console.WriteLine($"[RealmList] Realms: {realmCount}");

                for (int i = 0; i < realmCount; i++)
                {
                    byte icon = bodyReader.ReadByte();    // Realm icon/type
                    byte locked = bodyReader.ReadByte();  // Locked status
                    byte flags = bodyReader.ReadByte();   // Realm flags

                    string name = ReadCString(bodyReader);
                    string address = ReadCString(bodyReader);

                    float population = bodyReader.ReadSingle();
                    byte numChars = bodyReader.ReadByte();
                    byte timezone = bodyReader.ReadByte();
                    byte realmId = bodyReader.ReadByte();

                    realms.Add(new Realm
                    {
                        RealmType = icon,
                        Flags = flags,
                        RealmName = name,
                        AddressPort = int.TryParse(address.Split(':')[1], out var port) ? port : 3724,
                        Population = population,
                        NumChars = numChars,
                        RealmCategory = timezone,
                        RealmId = realmId
                    });
                }

                // Read trailing 2-byte null terminator
                ushort terminator = bodyReader.ReadUInt16();
                if (terminator != 0)
                {
                    Console.WriteLine($"[RealmList] Warning: Expected 0x0000 terminator, got 0x{terminator:X4}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginClient] Error parsing REALM_LIST: {ex}");
            }

            return realms;
        }

        private static string ReadCString(BinaryReader reader)
        {
            List<byte> buffer = [];
            byte b;
            while ((b = reader.ReadByte()) != 0)
                buffer.Add(b);
            return Encoding.UTF8.GetString(buffer.ToArray());
        }


        public void Dispose() { }
    }
}
