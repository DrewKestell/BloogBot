namespace RaidMemberBot.Models.Dto
{
    public class DatabaseRequest
    {
        public QueryType QueryType { get; set; }
        public string QueryParam1 { get; set; }
        public string QueryParam2 { get; set; }
        public string QueryParam3 { get; set; }
        public string QueryParam4 { get; set; }
    }

    public enum QueryType
    {
        GetItemById,
        GetCreatureMovementByGuid,
        GetCreatureLinkedByGuid,
        GetCreatureTemplateById,
        GetCreaturesById,
        GetCreaturesByMapId, 
        GetCreatureEquipTemplateById,
        GetDungeonStartingPoint
    }
}
