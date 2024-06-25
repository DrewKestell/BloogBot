using BaseSocketServer;
using MaNGOSDBDomain.Dto;
using MaNGOSDBDomain.Repository;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;

namespace MaNGOSDBDomain
{
    public class MaNGOSDBSocketServer : AbstractSocketServer
    {
        public MaNGOSDBSocketServer(int port, IPAddress ipAddress) : base(port, ipAddress)
        {
            Console.WriteLine($"[DATABASE SERVER : {port}] Started");
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
            return SendMessage(response, clientSocket);
        }
    }
}
