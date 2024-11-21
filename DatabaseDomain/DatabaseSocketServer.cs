using BotCommLayer;
using Database;

namespace DatabaseDomain
{
    public class DatabaseSocketServer(string ipAddress, int port, ILogger logger) : ProtobufSocketServer<DatabaseRequest, DatabaseResponse>(ipAddress, port, logger)
    {
        private readonly ILogger _logger = logger;

        // Factory method to get the appropriate handler based on table name
        private Func<DatabaseRequest, DatabaseResponse> GetTableHandler(TableName tableName)
        {
            return tableName switch
            {
                TableName.AreaTriggerBgEntrance => HandleAreaTriggerBgEntrance,
                TableName.AuctionHouseBot => HandleAuctionHouseBot,
                TableName.AreaTriggerInvolvedRelation => HandleAreaTriggerInvolvedRelation,
                TableName.AreaTriggerTavern => HandleAreaTriggerTavern,
                TableName.AreaTriggerTeleport => HandleAreaTriggerTeleport,
                TableName.AreaTemplate => HandleAreaTemplate,
                TableName.AutoBroadcast => HandleAutoBroadcast,

                // Add cases for other tables here...
                _ => HandleUnknownTable
            };
        }

        // Handle incoming request
        protected override DatabaseResponse HandleRequest(DatabaseRequest payload)
        {
            // Get the appropriate handler for the requested table
            var tableHandler = GetTableHandler(payload.TableName);

            // Pass the request payload to the handler and return the result
            return tableHandler(payload);
        }

        // Example handler for AreaTriggerBgEntrance table
        private DatabaseResponse HandleAreaTriggerBgEntrance(DatabaseRequest payload)
        {
            var response = new DatabaseResponse { TableName = TableName.AreaTriggerBgEntrance };

            return response;
        }

        // Example handler for AuctionHouseBot table
        private DatabaseResponse HandleAuctionHouseBot(DatabaseRequest payload)
        {
            var response = new DatabaseResponse { TableName = TableName.AuctionHouseBot };

            return response;
        }
        // Example handler for AreaTriggerInvolvedRelation table
        private DatabaseResponse HandleAreaTriggerInvolvedRelation(DatabaseRequest payload)
        {
            var response = new DatabaseResponse { TableName = TableName.AreaTriggerInvolvedRelation };

            return response;
        }
        // Example handler for AreaTriggerTavern table
        private DatabaseResponse HandleAreaTriggerTavern(DatabaseRequest payload)
        {
            var response = new DatabaseResponse { TableName = TableName.AreaTriggerTavern };

            return response;
        }

        // Example handler for AreaTriggerTeleport table
        private DatabaseResponse HandleAreaTriggerTeleport(DatabaseRequest payload)
        {
            var response = new DatabaseResponse { TableName = TableName.AreaTriggerTeleport };

            return response;
        }
        // Example handler for AreaTemplate table
        private DatabaseResponse HandleAreaTemplate(DatabaseRequest payload)
        {
            var response = new DatabaseResponse { TableName = TableName.AreaTemplate };

            return response;
        }

        // Example handler for AutoBroadcast table
        private DatabaseResponse HandleAutoBroadcast(DatabaseRequest payload)
        {
            var response = new DatabaseResponse { TableName = TableName.AutoBroadcast };

            return response;
        }

        // Fallback handler for unknown tables
        private DatabaseResponse HandleUnknownTable(DatabaseRequest payload)
        {
            return new DatabaseResponse
            {
                TableName = TableName.UnknownTable,
                ErrorMessage = "Unknown table requested"
            };
        }
    }
}
