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
            Console.WriteLine($"[DATABASE SERVER]Port {port}");
        }

        public override int HandleRequest(string payload, Socket clientSocket)
        {
            DatabaseRequest request = JsonConvert.DeserializeObject<DatabaseRequest>(payload);
            string response = "{}";

            switch (request.QueryType)
            {
                case QueryType.GetCreatureMovementByGuid:
                    response = JsonConvert.SerializeObject(MangosRepository.GetCreatureMovementById(int.Parse(request.QueryParam1)));
                    break;
                case QueryType.GetCreatureGroupingByMemberGuid:
                    response = JsonConvert.SerializeObject(MangosRepository.GetCreatureGroupingByMemberGuid(int.Parse(request.QueryParam1)));
                    break;
                case QueryType.GetCreatureTemplateById:
                    response = JsonConvert.SerializeObject(MangosRepository.GetCreatureTemplateById(ulong.Parse(request.QueryParam1)));
                    break;
                case QueryType.GetCreaturesById:
                    response = JsonConvert.SerializeObject(MangosRepository.GetCreaturesById(int.Parse(request.QueryParam1)));
                    break;
                case QueryType.GetCreaturesByMapId:
                    response = JsonConvert.SerializeObject(MangosRepository.GetCreaturesByMapId(int.Parse(request.QueryParam1)));
                    break;
                case QueryType.GetItemById:
                    response = JsonConvert.SerializeObject(MangosRepository.GetItemById(int.Parse(request.QueryParam1)));
                    break;
                case QueryType.GetCreatureEquipTemplateById:
                    response = JsonConvert.SerializeObject(MangosRepository.GetCreatureEquipTemplateById(int.Parse(request.QueryParam1)));
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
