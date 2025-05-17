using GameData.Core.Enums;

namespace GameData.Core.Interfaces
{
    public interface ISpell
    {
        uint Id { get; }

        uint Cost { get; }

        string Name { get; }

        string Description { get; }

        string Tooltip { get; }
    }
    public interface ISpellEffect
    {
        string Icon { get; }

        uint StackCount { get; }

        EffectType Type { get; }
    }
    public interface ICooldown
    {
        string Icon { get; }

        uint StackCount { get; }

        EffectType Type { get; }
    }
}
