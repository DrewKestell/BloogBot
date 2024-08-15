using BotRunner.Constants;

namespace BotRunner.Interfaces
{
    public interface IWoWPlayer : IWoWUnit
    {
        Race Race { get; }
        Class Class { get; }
        bool IsDrinking { get; }
        bool IsEating { get; }
    }
    public enum SpellModType
    {
        SPELLMOD_FLAT = 107,                      // SPELL_AURA_ADD_FLAT_MODIFIER
        SPELLMOD_PCT = 108                       // SPELL_AURA_ADD_PCT_MODIFIER
    }
}
