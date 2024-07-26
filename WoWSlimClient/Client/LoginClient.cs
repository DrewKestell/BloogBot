using Google.Protobuf;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using WoWSlimClient.Manager;
using WoWSlimClient.Models;
using WowSrp.Client;

namespace WoWSlimClient.Client
{
    internal class LoginClient(string ipAddress, int port) : IDisposable
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private IPAddress _ipAddress = IPAddress.Parse(ipAddress);
        private int _port = port;

        private TcpClient? _client = null;
        private SrpClient _srpClient;
        private SrpClientChallenge _srpClientChallenge;
        private NetworkStream _stream = null;

        private byte[] _serverProof = [];

        private static readonly BigInteger N = new("894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7", 16);
        private static readonly BigInteger g = BigInteger.ValueOf(7);
        private static readonly BigInteger k = CalculateK(N, g);

        public string Username => _username;
        public IPAddress IPAddress => _ipAddress;
        public int Port => _port;
        public byte[] ServerProof => _serverProof;
        public byte[] SessionKey => _srpClient.SessionKey;

        public void Connect()
        {
            try
            {
                _client?.Close();
                _client = new TcpClient(AddressFamily.InterNetwork);
                _client.Connect(_ipAddress, _port);
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

            WoWEventHandler.Instance.FireOnHandshakeBegin();
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

            writer.Write(Encoding.UTF8.GetBytes("68x\0")); // Platform: "\0x86" (actual bytes are "68x\0")
            writer.Write(Encoding.UTF8.GetBytes("niW\0")); // OS: "\0Win" (actual bytes are "niW\0")
            writer.Write(Encoding.UTF8.GetBytes("BGne")); // Locale: "enGB" (actual bytes are "BGne")

            writer.Write((uint)60); // Timezone Bias: 60 (UTC+1, little-endian)
            writer.Write((uint)0x0100007F); // Client IP: 127.0.0.1 (little-endian)

            writer.Write((byte)_username.Length); // Username length
            writer.Write(Encoding.UTF8.GetBytes(_username)); // Username

            writer.Flush(); 
            byte[] packetData = memoryStream.ToArray();

            // Send the packet
            _stream.Write(packetData, 0, packetData.Length);

            ReceiveAuthLogonChallengeServer();
        }

        private void ReceiveAuthLogonChallengeServer()
        {
            try
            {
                using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                byte[] packet = reader.ReadBytes(119); // Adjust length if necessary

                byte opcode = packet[0];
                byte result = packet[2];

                if (opcode == 0x00 && result == 0x00) // CMD_AUTH_LOGON_CHALLENGE and SUCCESS
                {
                    byte[] serverPublicKey = packet.Skip(3).Take(32).ToArray();
                    byte generator = packet[36];
                    byte largeSafePrimeLength = packet[37];
                    byte[] largeSafePrime = packet.Skip(38).Take(largeSafePrimeLength).ToArray();
                    byte[] salt = packet.Skip(70).Take(32).ToArray();
                    byte[] crcSalt = packet.Skip(102).Take(16).ToArray();

                    if (packet.Length > 118 && packet[118] == 0x01)
                    {
                        uint pinGridSeed = BitConverter.ToUInt32(packet, 119);
                        byte[] pinSalt = packet.Skip(123).Take(16).ToArray();
                        Console.WriteLine($"Two-Factor Authentication Enabled");
                    }

                    SendLogonProof(serverPublicKey, generator, largeSafePrime, salt, crcSalt);
                }
                else
                {
                    Console.WriteLine($"[LoginClient] Unexpected opcode or result received in AUTH_CHALLENGE response. [OpCode:{opcode:X}] [Result:{result:X}]");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginClient] An error occurred: {ex}");
            }
        }

        private void SendLogonProof(byte[] serverPublicKey, byte generator, byte[] largeSafePrime, byte[] salt, byte[] crcSalt)
        {
            try
            {
                _srpClientChallenge = new SrpClientChallenge(_username, _password, generator, largeSafePrime, serverPublicKey, salt);

                using SHA1 sha1 = SHA1.Create();
                byte[] crcHash = sha1.ComputeHash(Arrays.Concatenate(crcSalt, _srpClientChallenge.ClientProof));

                using var memoryStream = new MemoryStream();
                using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true);
                writer.Write((byte)0x01); // Opcode: CMD_AUTH_LOGON_PROOF

                writer.Write(_srpClientChallenge.ClientPublicKey);
                writer.Write(_srpClientChallenge.ClientProof);
                writer.Write(crcHash);

                writer.Write((byte)0x00); // Num keys: 0
                writer.Write((byte)0x00); // Two factor enabled: false

                writer.Flush();

                byte[] packetData = memoryStream.ToArray();
                _stream.Write(packetData, 0, packetData.Length);

                ReceiveAuthProofLogonResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginClient] An error occurred while sending logon proof: {ex}");
            }
        }

        private void ReceiveAuthProofLogonResponse()
        {
            try
            {
                using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                byte[] header = reader.ReadBytes(2);

                if (header.Length < 2)
                {
                    Console.WriteLine($"[LoginClient] Received incomplete AUTH_PROOF response packet.");
                }

                byte opcode = header[0];
                byte result = header[1];

                if (opcode == 0x01 && result == 0x00) // CMD_AUTH_LOGON_PROOF and SUCCESS
                {
                    byte[] body = reader.ReadBytes(24);
                    if (body.Length < 24)
                    {
                        Console.WriteLine("[LoginClient] Received incomplete success AUTH_PROOF response packet.");
                    }
                    byte[] serverProof = body.Take(20).ToArray();
                    _serverProof = serverProof;

                    var verificationResult = _srpClientChallenge.VerifyServerProof(serverProof);
                    if (!verificationResult.HasValue)
                    {
                        WoWEventHandler.Instance.FireOnLoginFailure();
                    }
                    else
                    {
                        _srpClient = verificationResult.Value;
                        WoWEventHandler.Instance.FireOnLoginSuccess();
                    }
                }
                else
                {
                    Console.WriteLine("[LoginClient] Authentication failed with result code: " + result);
                    WoWEventHandler.Instance.FireOnLoginFailure();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginClient] An error occurred while receiving AUTH_PROOF response: {ex}");
                WoWEventHandler.Instance.FireOnLoginFailure();
            }
        }

        private void SendRealmListRequest()
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true);
                writer.Write((ushort)0x10); // Packet size: 16 (big-endian)
                writer.Write((ushort)0x10); // Opcode: REALM_LIST (little-endian)
                writer.Write((ushort)0x00); // Padding (little-endian)

                writer.Flush();
                byte[] packetData = memoryStream.ToArray();
                _stream.Write(packetData, 0, packetData.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginClient] An error occurred while sending realm list request: {ex}");
            }
        }

        public List<Realm> GetRealmList()
        {
            List<Realm> list = [];

            SendRealmListRequest();

            try
            {
                using var reader = new BinaryReader(_stream, Encoding.UTF8, true);
                byte[] header = reader.ReadBytes(8); // Read the header first

                byte opcode = header[0];

                uint size = BitConverter.ToUInt16(header.Skip(1).Take(2).ToArray(), 0); // Little-endian
                uint numOfRealms = BitConverter.ToUInt16(header.Skip(6).Take(2).Reverse().ToArray(), 0); // Little-endian

                if (opcode == 0x10) // REALM_LIST
                {
                    byte[] body = reader.ReadBytes((int)(size - 7)); // Read the body of the packet

                    using var bodyStream = new MemoryStream(body);
                    using var bodyReader = new BinaryReader(bodyStream, Encoding.UTF8, true);
                    for (int i = 0; i < numOfRealms; i++)
                    {
                        list.Add(new Realm()
                        {
                            RealmType = bodyReader.ReadUInt32(),
                            Flags = bodyReader.ReadByte(),
                            RealmName = PacketManager.ReadCString(bodyReader),
                            AddressPort = int.Parse(PacketManager.ReadCString(bodyReader).Split(":")[1]),

                            Population = bodyReader.ReadSingle(),
                            NumChars = bodyReader.ReadByte(),
                            RealmCategory = bodyReader.ReadByte(),
                            RealmId = bodyReader.ReadByte(),
                        });
                    }
                }
                else
                {
                    Console.WriteLine("[LoginClient] Unexpected opcode received in REALM_LIST response.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginClient] An error occurred while receiving REALM_LIST response: {ex}");
            }

            return list;
        }
        private static BigInteger CalculateK(BigInteger N, BigInteger g)
        {
            Sha1Digest sha1 = new();
            byte[] hashInput = Arrays.Concatenate(N.ToByteArrayUnsigned(), g.ToByteArrayUnsigned());
            byte[] hash = new byte[sha1.GetDigestSize()];
            sha1.BlockUpdate(hashInput, 0, hashInput.Length);
            sha1.DoFinal(hash, 0);
            return new BigInteger(1, hash);
        }

        public void Dispose()
        {

        }
    }
}
