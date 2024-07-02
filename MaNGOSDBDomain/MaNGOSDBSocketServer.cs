using BaseSocketServer;
using MaNGOSDBDomain.Dto;
using MaNGOSDBDomain.Repository;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MaNGOSDBDomain
{
    public class MaNGOSDBSocketServer() : AbstractSocketServer(8080, IPAddress.Parse("127.0.0.1"))
    {
        public override int HandleRequest(byte[] payload, Socket clientSocket)
        {
            string parsedPayload = Encoding.UTF8.GetString(payload);
            DatabaseRequest request = JsonConvert.DeserializeObject<DatabaseRequest>(parsedPayload);
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
