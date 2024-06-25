namespace MaNGOSDBDomain.Dto
{
    public class DatabaseRequest
    {
        public QueryType QueryType { get; set; }
        public string QueryParam1 { get; set; } = string.Empty;
        public string QueryParam2 { get; set; } = string.Empty; 
        public string QueryParam3 { get; set; } = string.Empty;
        public string QueryParam4 { get; set; } = string.Empty;
    }

    public enum QueryType
    {
        GetItemById,
        GetCreatureMovementByGuid,
        GetCreatureGroupingByMemberGuid,
        GetCreatureTemplateById,
        GetCreaturesById,
        GetCreaturesByMapId,
        GetCreatureEquipTemplateById
    }
}
