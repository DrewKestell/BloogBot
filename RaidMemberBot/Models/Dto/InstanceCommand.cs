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
        SetAccountInfo,
        SetLevel,
        SetGearParams,
        AddSpell,
        SetActivity,
        SetRaidLeader,
        AddPartyMember,
        SetRaidMember,
        ConvertToRaid,
        ConvertToParty,
        GoTo,
        TeleTo,
        SetFacing,
        MoveForward,
        InteractWith,
        CastSpellOn,
        UseItemOn,
        ExecuteLuaCommand,
        BeginDungeon,
        MarkTarget,
        QueuePvP,
        Pull
    }
}
