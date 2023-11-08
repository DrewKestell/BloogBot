using Newtonsoft.Json;
using RaidMemberBot.Models;
using RaidMemberBot.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RaidMemberBot.Client
{
    public class DatabaseClient
    {
        private Socket _databaseSocket;

        readonly int BufferSize = 1024;

        public static DatabaseClient Instance { get; private set; } = new DatabaseClient();

        private DatabaseClient()
        {

        }

        public List<CreatureMovement> GetCreatureMovementByGuid(int guid)
        {
            DatabaseRequest databaseRequest = new DatabaseRequest()
            {
                QueryType = QueryType.GetCreatureMovementByGuid,
                QueryParam1 = guid.ToString()
            };

            string json = SendRequest(databaseRequest);
            return JsonConvert.DeserializeObject<List<CreatureMovement>>(json);
        }

        public List<CreatureLinking> GetCreatureLinkedByGuid(int id)
        {
            DatabaseRequest databaseRequest = new DatabaseRequest()
            {
                QueryType = QueryType.GetCreatureLinkedByGuid,
                QueryParam1 = id.ToString()
            };

            string json = SendRequest(databaseRequest);
            return JsonConvert.DeserializeObject<List<CreatureLinking>>(json);
        }

        public CreatureTemplate GetCreatureTemplateById(int guid)
        {
            DatabaseRequest databaseRequest = new DatabaseRequest()
            {
                QueryType = QueryType.GetCreatureTemplateById,
                QueryParam1 = guid.ToString()
            };

            string json = SendRequest(databaseRequest);
            return JsonConvert.DeserializeObject<CreatureTemplate>(json);
        }

        public List<Creature> GetCreaturesById(int id)
        {
            DatabaseRequest databaseRequest = new DatabaseRequest()
            {
                QueryType = QueryType.GetCreaturesById,
                QueryParam1 = id.ToString()
            };

            string json = SendRequest(databaseRequest);
            return JsonConvert.DeserializeObject<List<Creature>>(json);
        }

        internal List<Creature> GetCreaturesByMapId(int mapId)
        {
            DatabaseRequest databaseRequest = new DatabaseRequest()
            {
                QueryType = QueryType.GetCreaturesByMapId,
                QueryParam1 = mapId.ToString()
            };

            string json = SendRequest(databaseRequest);
            return JsonConvert.DeserializeObject<List<Creature>>(json);
        }
        internal CreatureEquipTemplate GetCreatureEquipTemplateById(int id)
        {
            DatabaseRequest databaseRequest = new DatabaseRequest()
            {
                QueryType = QueryType.GetCreatureEquipTemplateById,
                QueryParam1 = id.ToString()
            };

            string json = SendRequest(databaseRequest);
            return JsonConvert.DeserializeObject<CreatureEquipTemplate>(json);
        }
        internal AreaTriggerTeleport GetAreaTriggerTeleportByMapId(int mapId)
        {
            DatabaseRequest databaseRequest = new DatabaseRequest()
            {
                QueryType = QueryType.GetDungeonStartingPoint,
                QueryParam1 = mapId.ToString()
            };

            string json = SendRequest(databaseRequest);
            return JsonConvert.DeserializeObject<AreaTriggerTeleport>(json);
        }

        private string SendRequest(DatabaseRequest databaseRequest)
        {
            try
            {
                _databaseSocket ??= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (!_databaseSocket.Connected)
                {
                    try
                    {
                        _databaseSocket.Connect(IPAddress.Parse(RaidMemberSettings.Instance.ListenAddress), ConfigClient.Instance.ConfigurationResponse.DatabaseServerPort);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                if (_databaseSocket.Connected)
                {
                    string databaseRequestJson = JsonConvert.SerializeObject(databaseRequest);

                    _databaseSocket.Send(Encoding.ASCII.GetBytes(databaseRequestJson));

                    byte[] buffer = ReceiveMessage();

                    return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                try
                {
                    _databaseSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                catch
                {
                    _databaseSocket.Close();
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
                bytesReceived = _databaseSocket.Receive(buffer);

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
