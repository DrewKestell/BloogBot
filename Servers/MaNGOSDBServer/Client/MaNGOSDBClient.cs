using BaseSocketServer;
using MaNGOSDBDomain.Dto;
using MaNGOSDBDomain.Models;
using Newtonsoft.Json;
using System.Net;

namespace MaNGOSDBDomain.Client
{
    public class MaNGOSDBClient(int configPort, IPAddress ipAddress) : AbstractSocketClient(configPort, ipAddress)
    {
        public List<CreatureMovement> GetCreatureMovementByGuid(int guid)
        {
            DatabaseRequest databaseRequest = new()
            {
                QueryType = QueryType.GetCreatureMovementByGuid,
                QueryParam1 = guid.ToString()
            };

            string json = SendMessage(JsonConvert.SerializeObject(databaseRequest));
            return JsonConvert.DeserializeObject<List<CreatureMovement>>(json);
        }

        public List<CreatureGrouping> GetCreatureMappingByMemberGuid(int id)
        {
            DatabaseRequest databaseRequest = new()
            {
                QueryType = QueryType.GetCreatureGroupingByMemberGuid,
                QueryParam1 = id.ToString()
            };

            string json = SendMessage(JsonConvert.SerializeObject(databaseRequest));
            return JsonConvert.DeserializeObject<List<CreatureGrouping>>(json);
        }

        public CreatureTemplate GetCreatureTemplateById(int guid)
        {
            DatabaseRequest databaseRequest = new()
            {
                QueryType = QueryType.GetCreatureTemplateById,
                QueryParam1 = guid.ToString()
            };

            string json = SendMessage(JsonConvert.SerializeObject(databaseRequest));
            return JsonConvert.DeserializeObject<CreatureTemplate>(json);
        }

        public List<Creature> GetCreaturesById(int id)
        {
            DatabaseRequest databaseRequest = new()
            {
                QueryType = QueryType.GetCreaturesById,
                QueryParam1 = id.ToString()
            };

            string json = SendMessage(JsonConvert.SerializeObject(databaseRequest));
            return JsonConvert.DeserializeObject<List<Creature>>(json);
        }

        public List<Creature> GetCreaturesByMapId(int mapId)
        {
            DatabaseRequest databaseRequest = new()
            {
                QueryType = QueryType.GetCreaturesByMapId,
                QueryParam1 = mapId.ToString()
            };

            string json = SendMessage(JsonConvert.SerializeObject(databaseRequest));
            return JsonConvert.DeserializeObject<List<Creature>>(json);
        }
        public CreatureEquipTemplate GetCreatureEquipTemplateById(int id)
        {
            DatabaseRequest databaseRequest = new()
            {
                QueryType = QueryType.GetCreatureEquipTemplateById,
                QueryParam1 = id.ToString()
            };

            string json = SendMessage(JsonConvert.SerializeObject(databaseRequest));
            return JsonConvert.DeserializeObject<CreatureEquipTemplate>(json);
        }
    }
}
