using System.Text;
using static WoWSlimClient.Models.Enums;

namespace WoWSlimClient.Models
{
    public class Spell(int id, int cost, string name, string description, string tooltip)
    {
        public int Id { get; } = id;

        public int Cost { get; } = cost;

        public string Name { get; } = name;

        public string Description { get; } = description;

        public string Tooltip { get; } = tooltip;
    }
    public class SpellEffect(string icon, int stackCount, EffectType type)
    {
        public string Icon { get; } = icon;

        public int StackCount { get; } = stackCount;

        public EffectType Type { get; } = type;
    }
}
