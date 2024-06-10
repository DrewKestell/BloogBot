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
        FullStop,
        SetAccountInfo,
        SetLevel,
        SetReadyState,
        AddSpell,
        AddTalent,
        AddEquipment,
        SetActivity,
        SetRaidLeader,
        SetTankInPosition,
        AddPartyMember,
        ResetCharacterState,
        ConvertToRaid,
        ConvertToParty,
        GoTo,
        TeleTo,
        SetFacing,
        SetCombatLocation,
        MoveForward,
        InteractWith,
        CastSpellOn,
        UseItemOn,
        ExecuteLuaCommand,
        BeginDungeon,
        BeginBattleGrounds,
        BeginQuesting,
        BeginGathering,
        BeginWorldPvP,
        MarkTarget,
        AddRole,
        QueuePvP,
        Pull
    }
}
