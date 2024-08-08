using BotCommLayer;
using Database;

namespace DatabaseDomain.Client
{
    public class WoWDatabaseClient(string ipAddress, ILogger logger) : ProtobufSocketClient<DatabaseRequest, DatabaseResponse>(ipAddress, 8080, logger)
    {
        public List<CreatureMovement> GetCreatureMovementByGuid(int guid)
        {
            DatabaseRequest databaseRequest = new()
            {
                QueryType = QueryType.GetCreatureMovementByGuid,
                QueryParam1 = guid.ToString()
            };
            return [.. SendMessage(databaseRequest).CreatureMovement];
        }

        public List<CreatureGrouping> GetCreatureMappingByMemberGuid(int id)
        {
            DatabaseRequest databaseRequest = new()
            {
                QueryType = QueryType.GetCreatureGroupingByMemberGuid,
                QueryParam1 = id.ToString()
            };
            return [.. SendMessage(databaseRequest).CreatureGroupings];
        }

        public CreatureTemplate GetCreatureTemplateById(int guid)
        {
            DatabaseRequest databaseRequest = new()
            {
                QueryType = QueryType.GetCreatureTemplateById,
                QueryParam1 = guid.ToString()
            };
            return SendMessage(databaseRequest).CreatureTemplate;
        }

        public List<Creature> GetCreaturesById(int id)
        {
            DatabaseRequest databaseRequest = new()
            {
                QueryType = QueryType.GetCreaturesById,
                QueryParam1 = id.ToString()
            };
            return [.. SendMessage(databaseRequest).Creatures];
        }

        public List<Creature> GetCreaturesByMapId(int mapId)
        {
            DatabaseRequest databaseRequest = new()
            {
                QueryType = QueryType.GetCreaturesByMapId,
                QueryParam1 = mapId.ToString()
            };
            return [.. SendMessage(databaseRequest).Creatures];
        }
        public CreatureEquipTemplate GetCreatureEquipTemplateById(int id)
        {
            DatabaseRequest databaseRequest = new()
            {
                QueryType = QueryType.GetCreatureEquipTemplateById,
                QueryParam1 = id.ToString()
            };
            return SendMessage(databaseRequest).CreatureEquipTemplate;
        }
    }
}
