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
        SetReadyState,
        AddSpell,
        AddTalent,
        AddEquipment,
        SetActivity,
        SetRaidLeader,
        AddPartyMember,
        ResetCharacterState,
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
        AddRole,
        QueuePvP,
        Pull
    }
}
