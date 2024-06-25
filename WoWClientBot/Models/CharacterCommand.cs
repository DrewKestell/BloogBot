namespace WoWClientBot.Models
{
    public class CharacterCommand
    {
        public CharacterAction CharacterAction { get; set; }
        public string CommandParam1 { get; set; } = string.Empty;
        public string CommandParam2 { get; set; } = string.Empty;
        public string CommandParam3 { get; set; } = string.Empty;
        public string CommandParam4 { get; set; } = string.Empty;
    }

    public enum CharacterAction
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
        Pull,
        SetDatabaseConfig,
        SetNavigationConfig
    }
}
