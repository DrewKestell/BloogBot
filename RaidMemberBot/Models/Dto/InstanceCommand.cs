namespace RaidMemberBot.Models.Dto
{
    public class InstanceCommand
    {
        public static readonly string SET_ACCOUNT_INFO = "SET_ACCOUNT_INFO";
        public static readonly string START = "START";
        public static readonly string STOP = "STOP";
        public static readonly string SET = "SET";
        public static readonly string INVITE = "INVITE";
        public static readonly string JOIN = "JOIN";
        public static readonly string PULL = "PULL";
        public string CommandName { get; set; }
        public string CommandParam1 { get; set; }
        public string CommandParam2 { get; set; }
        public string CommandParam3 { get; set; }
        public string CommandParam4 { get; set; }
    }
}
