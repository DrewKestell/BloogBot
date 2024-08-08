using BotCommLayer;
using Database;
using DatabaseDomain.Repository;

namespace DatabaseDomain
{
    public class DatabaseSocketServer(string ipAddress, int port, ILogger logger) : ProtobufSocketServer<DatabaseRequest, DatabaseResponse>(ipAddress, port, logger)
    {
        protected override DatabaseResponse HandleRequest(DatabaseRequest payload)
        {
            DatabaseResponse databaseResponse = new ();
            switch (payload.QueryType)
            {
                case QueryType.GetCreatureMovementByGuid:
                    databaseResponse.CreatureMovement.AddRange(MangosRepository.GetCreatureMovementById(int.Parse(payload.QueryParam1)));
                    break;
                case QueryType.GetCreatureGroupingByMemberGuid:
                    databaseResponse.CreatureGroupings.AddRange(MangosRepository.GetCreatureGroupingByMemberGuid(int.Parse(payload.QueryParam1)));
                    break;
                case QueryType.GetCreatureTemplateById:
                    databaseResponse.CreatureTemplate = MangosRepository.GetCreatureTemplateById(ulong.Parse(payload.QueryParam1));
                    break;
                case QueryType.GetCreaturesById:
                    databaseResponse.Creatures.AddRange(MangosRepository.GetCreaturesById(int.Parse(payload.QueryParam1)));
                    break;
                case QueryType.GetCreaturesByMapId:
                    databaseResponse.Creatures.AddRange(MangosRepository.GetCreaturesByMapId(int.Parse(payload.QueryParam1)));
                    break;
                case QueryType.GetItemById:
                    databaseResponse.ItemTemplate = MangosRepository.GetItemById(int.Parse(payload.QueryParam1));
                    break;
                case QueryType.GetCreatureEquipTemplateById:
                    databaseResponse.CreatureEquipTemplate = MangosRepository.GetCreatureEquipTemplateById(int.Parse(payload.QueryParam1));
                    break;
            }
            return databaseResponse;
        }
    }
}
