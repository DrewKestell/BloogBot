using GameData.Core.Enums;
using GameData.Core.Interfaces;

namespace GameData.Core.Models
{
    public class Spell(uint id, uint cost, string name, string description, string tooltip) : ISpell
    {
        public uint Id { get; } = id;

        public uint Cost { get; } = cost;

        public string Name { get; } = name;

        public string Description { get; } = description;

        public string Tooltip { get; } = tooltip;
    }
    public class SpellEffect(string icon, uint stackCount, EffectType type) : ISpellEffect
    {
        public string Icon { get; } = icon;

        public uint StackCount { get; } = stackCount;

        public EffectType Type { get; } = type;
    }
    public class Cooldown(string icon, uint stackCount, EffectType type) : ICooldown
    {
        public string Icon { get; } = icon;

        public uint StackCount { get; } = stackCount;

        public EffectType Type { get; } = type;
    }
}
