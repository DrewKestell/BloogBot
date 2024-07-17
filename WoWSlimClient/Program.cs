using Ionic.Zlib;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using WowSrp.Client;
using WowSrp.Header;

class Program
{
    private static readonly BigInteger N = new BigInteger("894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7", 16);
    private static readonly BigInteger g = BigInteger.ValueOf(7);
    private static readonly BigInteger k = CalculateK(N, g);
    private static SrpClient srpClient;
    private static SrpClientChallenge srpClientChallenge;
    private static byte[] serverProof;
    private static VanillaDecryption vanillaDecryption;
    private static VanillaEncryption vanillaEncryption;

    static void Main()
    {
        NetworkStream stream = null;
        TcpClient loginClient = null;
        TcpClient worldClient = null;

        try
        {
            loginClient = new TcpClient(AddressFamily.InterNetwork);
            loginClient.Connect("127.0.0.1", 3724);
            stream = loginClient.GetStream();
            string accountName = "ORWR1"; // Replace with the actual account name

            SendAuthLogonChallengeClient(stream, accountName);

            worldClient = new TcpClient(AddressFamily.InterNetwork);
            worldClient.Connect("127.0.0.1", 8085); // Use the actual realm server address and port
            stream = worldClient.GetStream();
            HandleAuthChallenge(stream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex}");
        }
        finally
        {
            stream?.Dispose();
            loginClient?.Dispose();
        }

        Console.ReadKey();
    }

    // 1. Client asks for a login challenge by sending its build number and account name
    private static void SendAuthLogonChallengeClient(NetworkStream stream, string accountName)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true))
            {
                int packetSize = 30 + accountName.Length;

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

                writer.Write((byte)accountName.Length); // Username length
                writer.Write(Encoding.UTF8.GetBytes(accountName)); // Username

                writer.Flush();
                byte[] packetData = memoryStream.ToArray();
                stream.Write(packetData, 0, packetData.Length);

                Console.WriteLine($"Sent logon challenge packet");

                ReceiveAuthLogonChallengeServer(stream);
            }
        }
    }

    // 2. Server sends SRP authentication data
    private static void ReceiveAuthLogonChallengeServer(NetworkStream stream)
    {
        try
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                Console.WriteLine("Waiting to read from the stream...");

                byte[] packet = reader.ReadBytes(119); // Adjust length if necessary

                Console.WriteLine("Received packet: " + BitConverter.ToString(packet));

                byte opcode = packet[0];
                byte protocolVersion = packet[1];
                byte result = packet[2];

                Console.WriteLine($"Received opcode: {opcode}");
                Console.WriteLine($"Protocol Version: {protocolVersion}");
                Console.WriteLine($"Result: {result}");

                if (opcode == 0x00 && result == 0x00) // CMD_AUTH_LOGON_CHALLENGE and SUCCESS
                {
                    byte[] serverPublicKey = packet.Skip(3).Take(32).ToArray();
                    byte generatorLength = packet[35];
                    byte generator = packet[36];
                    byte largeSafePrimeLength = packet[37];
                    byte[] largeSafePrime = packet.Skip(38).Take(largeSafePrimeLength).ToArray();
                    byte[] salt = packet.Skip(70).Take(32).ToArray();
                    byte[] crcSalt = packet.Skip(102).Take(16).ToArray();

                    Console.WriteLine($"Server Public Key: {BitConverter.ToString(serverPublicKey)}");
                    Console.WriteLine($"Generator Length: {generatorLength}");
                    Console.WriteLine($"Generator: {generator}");
                    Console.WriteLine($"Large Safe Prime Length: {largeSafePrimeLength}");
                    Console.WriteLine($"Large Safe Prime: {BitConverter.ToString(largeSafePrime)}");
                    Console.WriteLine($"Salt: {BitConverter.ToString(salt)}");
                    Console.WriteLine($"CRC Salt: {BitConverter.ToString(crcSalt)}");

                    if (packet.Length > 118)
                    {
                        bool twoFactorEnabled = packet[118] == 0x01;
                        if (twoFactorEnabled)
                        {
                            uint pinGridSeed = BitConverter.ToUInt32(packet, 119);
                            byte[] pinSalt = packet.Skip(123).Take(16).ToArray();
                            Console.WriteLine($"Two-Factor Authentication Enabled");
                            Console.WriteLine($"PIN Grid Seed: {pinGridSeed:X}");
                            Console.WriteLine($"PIN Salt: {BitConverter.ToString(pinSalt)}");
                        }
                    }

                    SendLogonProof(stream, serverPublicKey, generator, largeSafePrime, salt, crcSalt);
                }
                else
                {
                    Console.WriteLine("Unexpected opcode or result received in AUTH_CHALLENGE response.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex}");
        }
    }

    // 3. Client sends SRP proof
    private static void SendLogonProof(NetworkStream stream, byte[] serverPublicKey, byte generator, byte[] largeSafePrime, byte[] salt, byte[] crcSalt)
    {
        try
        {
            string username = "ORWR1";
            string password = "password";

            srpClientChallenge = new SrpClientChallenge(username, password, generator, largeSafePrime, serverPublicKey, salt);

            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] crcHash = sha1.ComputeHash(Arrays.Concatenate(crcSalt, srpClientChallenge.ClientProof));

                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true))
                    {
                        writer.Write((byte)0x01); // Opcode: CMD_AUTH_LOGON_PROOF

                        writer.Write(srpClientChallenge.ClientPublicKey);
                        writer.Write(srpClientChallenge.ClientProof);
                        writer.Write(crcHash);

                        writer.Write((byte)0x00); // Num keys: 0
                        writer.Write((byte)0x00); // Two factor enabled: false

                        writer.Flush();
                    }

                    byte[] packetData = memoryStream.ToArray();
                    stream.Write(packetData, 0, packetData.Length);

                    Console.WriteLine($"Sent logon proof packet[{packetData.Length}]: {BitConverter.ToString(packetData)}");
                    Console.WriteLine($"Client Public Key[{srpClientChallenge.ClientPublicKey.Length}]: {BitConverter.ToString(srpClientChallenge.ClientPublicKey)}");
                    Console.WriteLine($"Client Proof[{srpClientChallenge.ClientProof.Length}]: {BitConverter.ToString(srpClientChallenge.ClientProof)}");
                    Console.WriteLine($"CRC Hash[{crcHash.Length}]: {BitConverter.ToString(crcHash)}");

                    ReceiveAuthProofLogonResponse(stream);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while sending logon proof: {ex}");
        }
    }

    // 4. Server accepts this proof and send its own, or reject and close the connection
    private static void ReceiveAuthProofLogonResponse(NetworkStream stream)
    {
        try
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                Console.WriteLine("Waiting to read AUTH_PROOF response from the stream...");

                byte[] header = reader.ReadBytes(2);

                if (header.Length < 2)
                {
                    Console.WriteLine($"Received incomplete AUTH_PROOF response packet.{BitConverter.ToString(header)}");
                    return;
                }

                byte opcode = header[0];
                byte result = header[1];

                Console.WriteLine("Received AUTH_PROOF response packet header: " + BitConverter.ToString(header));
                Console.WriteLine("Received AUTH_PROOF opcode: " + opcode);
                Console.WriteLine("Received AUTH_PROOF result: " + result);

                if (opcode == 0x01) // CMD_AUTH_LOGON_PROOF
                {
                    if (result == 0x00) // SUCCESS
                    {
                        byte[] body = reader.ReadBytes(24);
                        Console.WriteLine("Received AUTH_PROOF response packet body: " + BitConverter.ToString(body));
                        if (body.Length < 24)
                        {
                            Console.WriteLine("Received incomplete success AUTH_PROOF response packet.");
                            return;
                        }

                        serverProof = body.Take(20).ToArray();
                        byte[] hardwareSurveyID = body.Skip(20).Take(4).ToArray();

                        Console.WriteLine("Server Proof: " + BitConverter.ToString(serverProof));
                        Console.WriteLine("Hardware Survey ID: " + BitConverter.ToString(hardwareSurveyID));

                        var verificationResult = srpClientChallenge.VerifyServerProof(serverProof);
                        if (!verificationResult.HasValue)
                        {
                            Console.WriteLine("Server proof verification failed.");
                        }
                        else
                        {
                            Console.WriteLine("Authentication successful!");
                            srpClient = verificationResult.Value;
                            vanillaDecryption = new VanillaDecryption(srpClient.SessionKey);
                            vanillaEncryption = new VanillaEncryption(srpClient.SessionKey);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Authentication failed with result code: " + result);
                    }
                }
                else
                {
                    Console.WriteLine("Unexpected opcode received in AUTH_PROOF response.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while receiving AUTH_PROOF response: {ex}");
        }
    }

    private static void SendRealmListRequest(NetworkStream stream)
    {
        try
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true))
                {
                    writer.Write((ushort)0x10); // Packet size: 16 (big-endian)
                    writer.Write((ushort)0x10); // Opcode: REALM_LIST (little-endian)
                    writer.Write((ushort)0x00); // Padding (little-endian)

                    writer.Flush();
                    byte[] packetData = memoryStream.ToArray();
                    stream.Write(packetData, 0, packetData.Length);

                    Console.WriteLine($"Sent realm list request");

                    ReceiveRealmListResponse(stream);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while sending realm list request: {ex}");
        }
    }

    private static void ReceiveRealmListResponse(NetworkStream stream)
    {
        try
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                Console.WriteLine("Waiting to read REALM_LIST response from the stream...");

                byte[] header = reader.ReadBytes(8); // Read the header first

                Console.WriteLine($"Received REALM_LIST response header: {BitConverter.ToString(header)}");

                byte opcode = header[0];
                Console.WriteLine($"Received REALM_LIST opcode: {opcode}");

                uint size = BitConverter.ToUInt16(header.Skip(1).Take(2).ToArray(), 0); // Little-endian
                Console.WriteLine($"Received REALM_LIST size: {size}");

                uint numOfRealms = BitConverter.ToUInt16(header.Skip(6).Take(2).Reverse().ToArray(), 0); // Little-endian
                Console.WriteLine($"Received REALM_LIST numOfRealms: {numOfRealms}");

                if (opcode == 0x10) // REALM_LIST
                {
                    byte[] body = reader.ReadBytes((int)(size - 7)); // Read the body of the packet

                    Console.WriteLine($"Received REALM_LIST body: {BitConverter.ToString(body)}");

                    using (var bodyStream = new MemoryStream(body))
                    using (var bodyReader = new BinaryReader(bodyStream, Encoding.UTF8, true))
                    {
                        for (int i = 0; i < numOfRealms; i++)
                        {
                            uint realmType = bodyReader.ReadUInt32();
                            byte flags = bodyReader.ReadByte();

                            string realmName = ReadCString(bodyReader);
                            string addressPort = ReadCString(bodyReader);

                            float population = bodyReader.ReadSingle();
                            byte numChars = bodyReader.ReadByte();
                            byte realmCategory = bodyReader.ReadByte();
                            byte realmId = bodyReader.ReadByte();

                            Console.WriteLine($"Realm {i + 1} Info:");
                            Console.WriteLine($"Realm Type: {realmType}");
                            Console.WriteLine($"Flags: {flags}");
                            Console.WriteLine($"Realm Name: {realmName}");
                            Console.WriteLine($"Address: {addressPort}");
                            Console.WriteLine($"Population: {population}");
                            Console.WriteLine($"Number of Characters: {numChars}");
                            Console.WriteLine($"Realm Category: {realmCategory}");
                            Console.WriteLine($"Realm ID: {realmId}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Unexpected opcode received in REALM_LIST response.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while receiving REALM_LIST response: {ex}");
        }
    }

    private static void HandleNetworkMessages(NetworkStream stream)
    {
        try
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                while (true) // Loop to continuously read messages
                {
                    Console.WriteLine("Waiting to read from the world server...");

                    // Read the header first to determine the size and opcode
                    byte[] header = reader.ReadBytes(4); // Adjust size if header structure is different
                    if (header.Length < 4)
                    {
                        Console.WriteLine("Received incomplete packet header.");
                        break; // Exit if we cannot read a full header
                    }

                    HeaderData headerData = vanillaDecryption.ReadServerHeader(header);
                    Console.WriteLine($"Received opcode: {headerData.Opcode:X}, Size: {headerData.Size}");

                    // Read the packet body
                    byte[] body = reader.ReadBytes((int)(headerData.Size - sizeof(ushort))); // Adjust based on actual header size

                    Console.WriteLine($"Received body: {BitConverter.ToString(body)}");

                    // Switch based on the opcode to handle different messages
                    switch (headerData.Opcode)
                    {
                        case 0x1EE: // SMSG_AUTH_RESPONSE
                            HandleAuthResponse(stream, body);
                            break;
                        case 0x010: // REALM_LIST
                            HandleRealmList(body);
                            break;
                        case 0x03B: // SMSG_CHAR_ENUM
                            HandleCharEnum(stream, body); // Pass stream here
                            break;
                        default:
                            Console.WriteLine($"Unhandled opcode: {headerData.Opcode:X}");
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while handling network messages: {ex}");
        }
    }

    private static void HandleAuthChallenge(NetworkStream stream)
    {
        using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
        {
            Console.WriteLine("Processing SMSG_AUTH_CHALLENGE...");
            byte[] header = reader.ReadBytes(4);

            if (header.Length < 4)
            {
                Console.WriteLine($"Received incomplete SMSG_AUTH_CHALLENGE header.{BitConverter.ToString(header)}");
                return;
            }

            // 0x0 2 / Big uint16 size Size of the packet including the opcode field. Always 6.
            ushort size = BitConverter.ToUInt16(header.Take(2).Reverse().ToArray(), 0);
            // 0x2 2 / Little uint16 opcode Opcode for the packet. Determines the structure of the body. Always 0x1EC.
            ushort opcode = BitConverter.ToUInt16(header.Skip(2).Take(2).ToArray(), 0);

            Console.WriteLine($"Received SMSG_AUTH_CHALLENGE opcode: {opcode:X}");
            Console.WriteLine($"Received SMSG_AUTH_CHALLENGE size: {size}");
            byte[] serverSeed = reader.ReadBytes(size - sizeof(ushort));
            if (serverSeed.Length < 4)
            {
                Console.WriteLine($"Incomplete SMSG_AUTH_CHALLENGE packet. {BitConverter.ToString(serverSeed)} {size} {sizeof(ushort)}");
                return;
            }
            Console.WriteLine($"Received server seed: {BitConverter.ToString(serverSeed)}");

            // Normally, you would continue the authentication process by sending a response with the client's credentials.
            SendCMSGAuthSession(stream, serverSeed);
        }
    }

    private static void HandleAuthResponse(NetworkStream stream, byte[] body)
    {
        Console.WriteLine("Processing SMSG_AUTH_RESPONSE...");

        if (body.Length < 4)
        {
            Console.WriteLine("Incomplete SMSG_AUTH_RESPONSE packet.");
            return;
        }

        uint result = BitConverter.ToUInt32(body.Take(4).ToArray(), 0);
        Console.WriteLine($"Authentication result code: {result}");

        if (result == 0x0C) // AUTH_OK
        {
            Console.WriteLine($"Authentication successful! {BitConverter.ToString(body)}");
            //uint billingTime = BitConverter.ToUInt32(body, 4);
            //byte billingFlags = body[8];
            //uint billingRested = BitConverter.ToUInt32(body, 9);

            //Console.WriteLine($"Billing Time: {billingTime}");
            //Console.WriteLine($"Billing Flags: {billingFlags}");
            //Console.WriteLine($"Billing Rested: {billingRested}");
            // Proceed with next steps, such as requesting character lists
            SendCMSGCharEnum(stream);
        }
        else
        {
            Console.WriteLine("Authentication failed.");
        }
    }

    private static void HandleRealmList(byte[] body)
    {
        Console.WriteLine("Processing REALM_LIST...");

        // Assuming the first two bytes after the opcode indicate the number of realms
        if (body.Length < 2)
        {
            Console.WriteLine("Incomplete REALM_LIST packet.");
            return;
        }

        int numberOfRealms = BitConverter.ToInt16(body.Take(2).ToArray(), 0);
        Console.WriteLine($"Number of realms: {numberOfRealms}");

        // Additional logic to parse and display realm details
    }

    private static void HandleCharEnum(NetworkStream stream, byte[] body)
    {
        Console.WriteLine("Processing SMSG_CHAR_ENUM...");

        // Here, parse the body to extract character details
        using (var memoryStream = new MemoryStream(body))
        using (var reader = new BinaryReader(memoryStream))
        {
            byte numOfCharacters = reader.ReadByte();
            Console.WriteLine($"Number of characters: {numOfCharacters}");
            ulong guid = 0;
            for (int i = 0; i < numOfCharacters; i++)
            {
                // Extract character details
                guid = reader.ReadUInt64(); // 8 bytes for GUID
                string name = ReadCString(reader);
                byte race = reader.ReadByte();
                byte characterClass = reader.ReadByte();
                byte gender = reader.ReadByte();
                byte skin = reader.ReadByte();
                byte face = reader.ReadByte();
                byte hairStyle = reader.ReadByte();
                byte hairColor = reader.ReadByte();
                byte facialHair = reader.ReadByte();
                byte level = reader.ReadByte();

                uint zoneId = reader.ReadUInt32();
                uint mapId = reader.ReadUInt32();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();

                // Equipment data: 20 items, 2 bytes each
                byte[] equipment = reader.ReadBytes(20 * 2);

                Console.WriteLine($"Character {i + 1}:");
                Console.WriteLine($"  GUID: {guid}");
                Console.WriteLine($"  Name: {name}");
                Console.WriteLine($"  Race: {race}");
                Console.WriteLine($"  Class: {characterClass}");
                Console.WriteLine($"  Gender: {gender}");
                Console.WriteLine($"  Level: {level}");
                Console.WriteLine($"  Zone ID: {zoneId}");
                Console.WriteLine($"  Map ID: {mapId}");
                Console.WriteLine($"  Location: ({x}, {y}, {z})");
                Console.WriteLine($"  Equipment: {BitConverter.ToString(equipment)}");
            }

            // Example: Enter the world with the first character
            if (numOfCharacters > 0)
            {
                SendCMSGPlayerLogin(stream, guid);
            }
        }
    }

    private static void SendCMSGAuthSession(NetworkStream stream, byte[] serverSeed)
    {
        try
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true))
                {
                    uint opcode = 0x01ED; // Opcode for CMSG_AUTH_SESSION
                    uint build = 5875; // Revision of the client
                    uint serverId = 1; // Server ID, this value may vary
                    string username = "ORWR1"; // Replace with actual username
                    uint clientSeed = (uint)new Random().Next(int.MinValue, int.MaxValue); // Generate a random client seed

                    srpClient = srpClientChallenge.VerifyServerProof(serverProof).Value;
                    if (srpClient.SessionKey.Length < 1)
                    {
                        Console.WriteLine("SrpClient is null. Cannot proceed with authentication.");
                        return;
                    }

                    byte[] clientProof = GenerateClientProof(username, clientSeed, serverSeed, srpClient.SessionKey); // Generate the client proof
                    byte[] decompressedAddonInfo = GenerateAddonInfo(); // Generate addon info
                    byte[] compressedAddonInfo = CompressAddonInfo(decompressedAddonInfo); // Compress the addon info
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
                    stream.Write(packetData, 0, packetData.Length);

                    Console.WriteLine($"Sent CMSG_AUTH_SESSION packet:{packetSize} {packetData.Length} {BitConverter.ToString(packetData)}");

                    HandleNetworkMessages(stream);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while sending CMSG_AUTH_SESSION: {ex}");
        }
    }

    private static void SendCMSGCharEnum(NetworkStream stream)
    {
        try
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true))
                {
                    byte[] header = vanillaEncryption.CreateClientHeader(4, 0x037);
                    writer.Write(header); // Packet size: 4 (big-endian)

                    writer.Flush();
                    byte[] packetData = memoryStream.ToArray();

                    stream.Write(packetData, 0, packetData.Length);

                    Console.WriteLine($"Sent CMSG_CHAR_ENUM packet: {BitConverter.ToString(packetData)}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while sending CMSG_CHAR_ENUM: " + ex.Message);
        }
    }

    private static void SendCMSGPlayerLogin(NetworkStream stream, ulong characterGuid)
    {
        try
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true))
                {
                    byte[] header = vanillaEncryption.CreateClientHeader(12, 0x03D);
                    writer.Write(header); // Opcode: CMSG_PLAYER_LOGIN
                    writer.Write(characterGuid); // Character GUID

                    writer.Flush();
                    byte[] packetData = memoryStream.ToArray();
                    stream.Write(packetData, 0, packetData.Length);

                    Console.WriteLine($"Sent CMSG_PLAYER_LOGIN packet: {BitConverter.ToString(packetData)}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while sending CMSG_PLAYER_LOGIN: " + ex.Message);
        }
    }

    private static string ReadCString(BinaryReader reader)
    {
        StringBuilder sb = new StringBuilder();
        char c;
        while ((c = reader.ReadChar()) != '\0')
        {
            sb.Append(c);
        }
        return sb.ToString();
    }

    private static BigInteger CalculateK(BigInteger N, BigInteger g)
    {
        Sha1Digest sha1 = new Sha1Digest();
        byte[] hashInput = Arrays.Concatenate(N.ToByteArrayUnsigned(), g.ToByteArrayUnsigned());
        byte[] hash = new byte[sha1.GetDigestSize()];
        sha1.BlockUpdate(hashInput, 0, hashInput.Length);
        sha1.DoFinal(hash, 0);
        return new BigInteger(1, hash);
    }

    private static byte[] GenerateClientProof(string username, uint clientSeed, byte[] serverSeed, byte[] sessionKey)
    {
        using (SHA1 sha1 = SHA1.Create())
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(Encoding.UTF8.GetBytes(username));
                    writer.Write(new byte[4]); // Write t (all zeroes)
                    writer.Write(clientSeed);
                    writer.Write(serverSeed);
                    writer.Write(sessionKey ?? Array.Empty<byte>());

                    byte[] hash = sha1.ComputeHash(ms.ToArray());
                    return hash;
                }
            }
        }
    }

    private static byte[] GenerateAddonInfo()
    {
        string addonData =
            "Blizzard_AuctionUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
            "Blizzard_BattlefieldMinimap\x00\x01mw\x1cL\x00\x00\x00\x00" +
            "Blizzard_BindingUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
            "Blizzard_CombatText\x00\x01mw\x1cL\x00\x00\x00\x00" +
            "Blizzard_CraftUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
            "Blizzard_GMSurveyUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
            "Blizzard_InspectUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
            "Blizzard_MacroUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
            "Blizzard_RaidUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
            "Blizzard_TalentUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
            "Blizzard_TradeSkillUI\x00\x01mw\x1cL\x00\x00\x00\x00" +
            "Blizzard_TrainerUI\x00\x01mw\x1cL\x00\x00\x00\x00";

        return Encoding.UTF8.GetBytes(addonData);
    }

    private static byte[] CompressAddonInfo(byte[] addonInfo)
    {
        // Compress the addon data using Zlib
        using (var outputStream = new MemoryStream())
        {
            using (var compressionStream = new ZlibStream(outputStream, CompressionMode.Compress, CompressionLevel.Default))
            {
                compressionStream.Write(addonInfo, 0, addonInfo.Length);
            }
            return outputStream.ToArray();
        }
    }
}
