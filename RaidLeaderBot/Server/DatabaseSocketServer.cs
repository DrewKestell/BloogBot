using Newtonsoft.Json;
using RaidMemberBot.Models.Dto;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RaidLeaderBot
{
    public class DatabaseSocketServer : BaseSocketServer
    {
        public DatabaseSocketServer(int port, IPAddress ipAddress) : base(port, ipAddress)
        {
            Console.WriteLine($"DATABASE SERVER:Port {port}");
        }

        public override int HandleRequest(string payload, Socket clientSocket)
        {
            DatabaseRequest request = JsonConvert.DeserializeObject<DatabaseRequest>(payload);
            string response = "{}";

            switch (request.QueryType)
            {
                case QueryType.GetCreatureMovementByGuid:
                    response = JsonConvert.SerializeObject(SqliteRepository.GetCreatureMovementById(int.Parse(request.QueryParam1)));
                    break;
                case QueryType.GetCreatureLinkedByGuid:
                    response = JsonConvert.SerializeObject(SqliteRepository.GetCreatureLinkedByGuid(int.Parse(request.QueryParam1)));
                    break;
                case QueryType.GetCreatureTemplateById:
                    response = JsonConvert.SerializeObject(SqliteRepository.GetCreatureTemplateById(ulong.Parse(request.QueryParam1)));
                    break;
                case QueryType.GetCreaturesById:
                    response = JsonConvert.SerializeObject(SqliteRepository.GetCreaturesById(int.Parse(request.QueryParam1)));
                    break;
                case QueryType.GetCreaturesByMapId:
                    response = JsonConvert.SerializeObject(SqliteRepository.GetCreaturesByMapId(int.Parse(request.QueryParam1)));
                    break;
                case QueryType.GetItemById:
                    response = JsonConvert.SerializeObject(SqliteRepository.GetItemById(ulong.Parse(request.QueryParam1)));
                    break;
                case QueryType.GetDungeonStartingPoint:
                    response = JsonConvert.SerializeObject(SqliteRepository.GetAreaTriggerTeleportByMapId(int.Parse(request.QueryParam1)));
                    break;
                case QueryType.GetCreatureEquipTemplateById:
                    response = JsonConvert.SerializeObject(SqliteRepository.GetCreatureEquipTemplateById(int.Parse(request.QueryParam1)));
                    break;
            }
            byte[] bytes = Encoding.ASCII.GetBytes(response);
            while (bytes.Length > 1024)
            {
                byte[] tempBytes = new byte[1024];
                Array.Copy(bytes, tempBytes, 1024);

                clientSocket.Send(tempBytes);

                Array.Reverse(bytes);
                Array.Resize(ref bytes, bytes.Length - 1024);
                Array.Reverse(bytes);
            }
            if (bytes.Length > 0)
            {
                clientSocket.Send(bytes);
            }
            return 0;
        }
    }
}
