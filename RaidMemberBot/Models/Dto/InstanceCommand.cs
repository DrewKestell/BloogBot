namespace RaidMemberBot.Models.Dto
{
    public class InstanceCommand
    {
        public CommandAction CommandAction { get; set; }
        public string CommandParam1 { get; set; }
        public string CommandParam2 { get; set; }
        public string CommandParam3 { get; set; }
        public string CommandParam4 { get; set; }
    }

    public enum CommandAction
    {
        None,
        SetCharacterParams,
        SetGearParams,
        SetTalentParams,
        SetSkillsParams,
        SetActivity,
        SetRaidLeader,
        AddPartyMember,
        SetRaidMember,
        ConvertToRaid,
        ConvertToParty,
        GoTo,
        TeleportTo,
        InteractWithWoWObject,
        UseItemOnWoWObject,
        BeginDungeon,
        MarkTarget,
        QueuePvP,
        Pull
    }
}
